using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BTree
{
  public class BTreePersistant : BTreeVolatile
  {
    // Répertoire dans lequel sont stockés les fichiers de données
    private readonly DirectoryInfo Repertoire;
    private List<NiveauPersistant> Niveaux;
    protected override int Profondeur => Niveaux.Count - 1;
    public BTreePersistant(DirectoryInfo repertoire, int tailleElement, int ordre, IComparateurSituations cmp) : base(tailleElement, ordre, cmp)
    {
      Repertoire = repertoire;
    }
    public override void InitBTree()
    {
      // Assure son initialisation si 0.dat n'existe pas
      ChargeStructure();
      // Initialisation des parties utiles lors des splits pendant les insertions
      PileInsertion = new List<DataParcourtVolatile>();
      for (int idxPileInsertion = 0; idxPileInsertion <= Profondeur; idxPileInsertion++)
      {
        PileInsertion.Add(new DataParcourtPersistant());
      }
      dataRemontee = new DataRemonteePersistant(TailleElement);
      dataRemonteeSuivante = new DataRemonteePersistant(TailleElement);
    }
    private void ChargeStructure()
    {
      // Initialisation si nécessaire
      if (!Repertoire.Exists)
      {
        Repertoire.Create();
      }
      FileInfo fi0 = new FileInfo(Path.Combine(Repertoire.FullName, "0.dat"));
      if (!fi0.Exists)
      {
        using (FileStream sw = new FileStream(fi0.FullName, FileMode.CreateNew))
        {
          NoeudPersistant feuille = new NoeudPersistant(Ordre, TailleElement, true);
          sw.Write(feuille.data, 0, feuille.data.Length);
        }
      }
      // Chargement des fichiers *.dat
      Niveaux = new List<NiveauPersistant>();
      FileInfo[] listeFichiers = Repertoire.GetFiles();
      int[] numeroFichiers = new int[listeFichiers.Length];
      int idxFichier = 0;
      foreach (var fi in listeFichiers)
      {
        // Vérification du nom des fichiers qui doit être <n>.dat
        Match m = Regex.Match(fi.Name, @"^(\d+)\.dat$");
        if (!m.Success)
        {
          throw new ApplicationException($"Le fichier {fi.FullName} a un nom incorrect");
        }
        numeroFichiers[idxFichier++] = int.Parse(m.Groups[1].Value);
      }
      // Tri de ces fichiers selon le n° de leur nom
      Array.Sort(numeroFichiers, listeFichiers);
      // vérification qu'il n'y a pas de trous dans la numérotation
      for (idxFichier = 0; idxFichier < numeroFichiers.Length; idxFichier++)
      {
        if (numeroFichiers[idxFichier] != idxFichier)
        {
          throw new ApplicationException($"Les fichiers de {Repertoire.FullName} n'ont pas une séquence correcte");
        }
      }
      // Ouverture de ces fichiers
      for (idxFichier = 0; idxFichier < listeFichiers.Length; idxFichier++)
      {
        Niveaux.Add(new NiveauPersistant(listeFichiers[idxFichier], Ordre, TailleElement, idxFichier == 0));
      }
      // Chargement du contenu de la racine
      Racine = ((NiveauPersistant)Niveaux[Profondeur]).ChargeNoeud(0);
    }

    internal override void SwitchData()
    {
      DataRemonteePersistant tmpDataRemontee = (DataRemonteePersistant)dataRemontee;
      // eltAInserer = eltRemonte
      dataRemontee = dataRemonteeSuivante;
      // noeudSuperieur = nouveauNoeud
      dataRemonteeSuivante = tmpDataRemontee;
    }
    public unsafe InsertOrUpdateResult InsertOrUpdate(byte[] element)
    {
      fixed (byte* pElement = element)
      {
        return InsertOrUpdate(pElement);
      }
    }
    internal override void CreeNouvelleRacine(DataRemonteeVolatile dataRemontee)
    {
      int nouvelleProfondeur = Profondeur + 1;
      NoeudPersistant nouvelleRacine = new NoeudPersistant(Ordre, TailleElement, false);
      Array.Copy(dataRemontee.elementRemonte, 0, nouvelleRacine.data, 1, TailleElement);
      nouvelleRacine.Enfants[0] = Racine;
      nouvelleRacine.OffsetEnfants[0] = 0;
      nouvelleRacine.Enfants[1] = dataRemontee.enfantPlus;
      nouvelleRacine.OffsetEnfants[1] = ((DataRemonteePersistant)dataRemontee).offsetEnfantPlus;
      nouvelleRacine.NbElements = 1;
      FileInfo fi0 = new FileInfo(Path.Combine(Repertoire.FullName, $"{nouvelleProfondeur}.dat"));
      using (FileStream sw = new FileStream(fi0.FullName, FileMode.CreateNew))
      {
        sw.Write(nouvelleRacine.data, 0, nouvelleRacine.data.Length);
        using (BinaryWriter bw = new BinaryWriter(sw))
        {
          for (int idxOffset = 0; idxOffset < Ordre; idxOffset++)
          {
            bw.Write(nouvelleRacine.OffsetEnfants[idxOffset]);
          }
        }
      }
      Niveaux.Add(new NiveauPersistant(fi0, Ordre, TailleElement, nouvelleProfondeur == 0));
      Racine = nouvelleRacine;
      PileInsertion.Add(new DataParcourtPersistant());
    }
    internal override NoeudVolatile GetNoeudEnfant(NoeudVolatile noeud, int profondeurNoeud, int idxEnfant)
    {
      if (noeud.Enfants[idxEnfant] == null)
      {
        noeud.Enfants[idxEnfant] = ((NiveauPersistant)Niveaux[profondeurNoeud - 1]).ChargeNoeud(((NoeudPersistant)noeud).OffsetEnfants[idxEnfant]);
      }
      return noeud.Enfants[idxEnfant];
    }
    internal override void AlloueNoeud(int idxProfondeur, DataRemonteeVolatile data)
    {
      Niveaux[idxProfondeur].AlloueNoeud(data);
    }
    public void Flush()
    {
      Flush((NoeudPersistant)Racine, Profondeur, 0);
    }

    private void Flush(NoeudPersistant noeud, int profondeur, UInt32 offsetNoeud)
    {
      if (noeud.dirty)
      {
        ((NiveauPersistant)Niveaux[profondeur]).Flush(noeud, offsetNoeud);
      }
      if (!noeud.IsFeuille)
      {
        for (int idxEnfant = 0; idxEnfant <= noeud.NbElements; idxEnfant++)
        {
          Flush((NoeudPersistant)noeud.Enfants[idxEnfant], profondeur - 1, (noeud).OffsetEnfants[idxEnfant]);
        }
      }
    }
    public void Close()
    {
      for (int idxProfondeur = 0; idxProfondeur <= Profondeur; idxProfondeur++)
      {
        ((NiveauPersistant)Niveaux[idxProfondeur]).Close();
      }
    }
  }
}
