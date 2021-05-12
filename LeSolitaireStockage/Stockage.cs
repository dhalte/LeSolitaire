using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LeSolitaireStockage
{
  public partial class Stockage
  {
    // Répertoire dans lequel sont stockés les fichiers de données
    private readonly DirectoryInfo Repertoire;
    // taille en octets d'un élément
    private readonly int TailleElement;
    // 1+nb max d'éléments, nb max liens vers noeuds enfants
    private readonly int Ordre;
    // constante utilisée lors d'un split de noeud
    private readonly int IdxPivot;
    // fonction de rappel pour comparaison d'éléments, et éventuelle mise à jour de la partie variable
    private readonly ComparateurSituations Comparateur;
    // Les différents niveaux du B-Tree, de 0, niveau des feuilles, à <Profondeur>, profondeur de l'arbre.
    private List<StockageNiveau> Niveaux;
    // Profondeur == 0 pour les feuilles
    private int Profondeur => Niveaux.Count - 1;
    private bool NoeudComplet(StockageNoeud noeud) => noeud.data[0] == Ordre - 1;

    StockageNoeud Racine;
    // Lors d'une recherche pour insertion, permet de conserver les informations nécessaires sans utiliser la récurrence.
    // Cette "pile" est de la taille Profondeur+1 et augmente lors d'un split de la racine.
    private class DataInsertion
    {
      internal StockageNoeud noeudActuel;
      internal int idxEnfant;
    }
    List<DataInsertion> PileInsertion;
    // utilisé pour stocker les informations qui remontent au parent lors d'un split d'un de ses enfants.
    private class DataRemontee
    {
      internal byte[] elementRemonte;
      internal UInt32 offsetEnfantPlus;
      internal StockageNoeud enfantPlus;

      public DataRemontee(int tailleElement)
      {
        elementRemonte = new byte[tailleElement];
      }
    }
    DataRemontee dataRemontee, dataRemonteeSuivante;
    public Stockage(DirectoryInfo repertoire, int tailleElement, int ordre, ComparateurSituations cmp/*, bool readOnly*/)
    {
      Repertoire = repertoire;
      TailleElement = tailleElement;
      Ordre = ordre;
      // division euclidienne
      IdxPivot = ordre / 2 - 1;
      Comparateur = cmp;
      // Assure son initialisation si 0.dat n'existe pas
      ChargeStructure();
      // Initialisation des parties utiles lors des splits pendant les insertions
      //if (!readOnly)
      //{
      PileInsertion = new List<DataInsertion>();
      for (int idxPileInsertion = 0; idxPileInsertion <= Profondeur; idxPileInsertion++)
      {
        PileInsertion.Add(new DataInsertion());
      }
      dataRemontee = new DataRemontee(TailleElement);
      dataRemonteeSuivante = new DataRemontee(TailleElement);
      //}
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
          StockageNoeud feuille = new StockageNoeud(Ordre, TailleElement, true);
          sw.Write(feuille.data, 0, feuille.data.Length);
        }
      }
      // Chargement des fichiers *.dat
      Niveaux = new List<StockageNiveau>();
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
        Niveaux.Add(new StockageNiveau(idxFichier, listeFichiers[idxFichier], Ordre, TailleElement, false));
      }
      // Chargement du contenu de la racine
      Racine = Niveaux[Profondeur].ChargeNoeud(0);
    }

    public unsafe InsertOrUpdateResult InsertOrUpdate(byte* pSituation)
    {
      // Cas particulier de l'arbre vide
      if (Racine.NbElements == 0)
      {
        Racine.Insert(pSituation, 0, TailleElement, null, UInt32.MaxValue);
        return InsertOrUpdateResult.Inserted;
      }
      // Recherche de la présence dans l'arbre de la situation passée en paramètre, avec mémoire du chemin parcouru pour la trouver
      int idxProfondeur;
      StockageNoeud noeudActuel = Racine;
      for (idxProfondeur = Profondeur; idxProfondeur >= 0; idxProfondeur--)
      {
        bool bFound = RechercheDichotomique(pSituation, noeudActuel, out int idxPosition);
        if (bFound)
        {
          // La situation en paramètre est déjà présente dans l'arbre. idxPosition est alors >= 0
          fixed (byte* pSituationExistante = &noeudActuel.data[1 + idxPosition * TailleElement])
          {
            // On donne une chance à l'appelant de modifier les données variables de la situation stockée dans l'arbre
            bool bModifie = Comparateur.MajSituation(pSituation, pSituationExistante);
            if (bModifie)
            {
              noeudActuel.dirty = true;
              return InsertOrUpdateResult.Updated;
            }
            return InsertOrUpdateResult.NoChange;
          }
        }
        // mise en mémoire du chemin suivi lors de la recherche de la situation passée en paramètre
        PileInsertion[idxProfondeur].noeudActuel = noeudActuel;
        PileInsertion[idxProfondeur].idxEnfant = idxPosition;
        if (idxProfondeur > 0)
        {
          StockageNoeud noeudEnfant = GetNoeudEnfant(noeudActuel, idxProfondeur, idxPosition + 1);
          noeudActuel = noeudActuel.Enfants[idxPosition + 1];
        }
      }
      // Si on arrive ici, c'est qu'on n'a pas trouvé la situation passée en paramètre
      // alors le chemin suivi nous a mené à une feuille dans laquelle la situation doit prendre place
      StockageNoeud nouveauNoeud = null;
      UInt32 offsetNouveauNoeud = UInt32.MaxValue;
      for (int idxData = 0; idxData < TailleElement; idxData++)
      {
        dataRemontee.elementRemonte[idxData] = pSituation[idxData];
      }
      dataRemontee.enfantPlus = null;
      dataRemontee.offsetEnfantPlus = UInt32.MaxValue;
      for (idxProfondeur = 0; idxProfondeur <= Profondeur; idxProfondeur++)
      {
        DataInsertion pop = PileInsertion[idxProfondeur];
        if (!NoeudComplet(pop.noeudActuel))
        {
          fixed (byte* pElementRemonte = dataRemontee.elementRemonte)
          {
            pop.noeudActuel.Insert(pElementRemonte, (byte)(pop.idxEnfant + 1), TailleElement, dataRemontee.enfantPlus, dataRemontee.offsetEnfantPlus);
          }
          return InsertOrUpdateResult.Inserted;
        }
        // Allocation d'un nouveau noeud dans la profondeur actuelle
        nouveauNoeud = Niveaux[idxProfondeur].AlloueNoeud(out offsetNouveauNoeud);
        dataRemonteeSuivante.enfantPlus = nouveauNoeud;
        dataRemonteeSuivante.offsetEnfantPlus = offsetNouveauNoeud;
        // split du noeud
        // Choix de l'élément pivot, qui va remonter dans le noeud parent
        int idxInsertion = pop.idxEnfant + 1;
        if (idxInsertion <= IdxPivot)
        {
          // eltRemonte = noeudActuel[idxPivot] ' copie de l’élément qui va remonter dans le parent
          Array.Copy(pop.noeudActuel.data, 1 + TailleElement * IdxPivot, dataRemonteeSuivante.elementRemonte, 0, TailleElement);
          // Copie de noeudActuel[idxPivot + 1 .. Ordre – 2] --> nouveauNoeud[0 .. Ordre – 3 – idxPivot] ' nb = Ordre – 2 – idxPivot
          Array.Copy(pop.noeudActuel.data, 1 + TailleElement * (IdxPivot + 1), nouveauNoeud.data, 1, TailleElement * (Ordre - 2 - IdxPivot));
          if (!pop.noeudActuel.IsFeuille)
          {
            // copie de noeudActuel.Enfants[idxPivot + 1 .. Ordre – 1] --> nouveauNoeud.Enfants[0 .. Ordre – 2 – idxPivot] ' nb = Ordre – 1 – idxPivot
            Array.Copy(pop.noeudActuel.Enfants, IdxPivot + 1, nouveauNoeud.Enfants, 0, Ordre - 1 - IdxPivot);
            Array.Copy(pop.noeudActuel.OffsetEnfants, IdxPivot + 1, nouveauNoeud.OffsetEnfants, 0, Ordre - 1 - IdxPivot);
          }
          // Déplacement de 1 case à droite de noeudActuel[idxInsertion .. idxPivot – 1] 
          Array.Copy(pop.noeudActuel.data, 1 + TailleElement * idxInsertion, pop.noeudActuel.data, 1 + TailleElement * (idxInsertion + 1), TailleElement * (IdxPivot - idxInsertion));
          if (!pop.noeudActuel.IsFeuille)
          {
            // déplacement de 1 case à droite de noeudActuel.Enfants[idxInsertion + 1 .. idxPivot]
            Array.Copy(pop.noeudActuel.Enfants, idxInsertion + 1, pop.noeudActuel.Enfants, idxInsertion + 2, IdxPivot - idxInsertion);
            Array.Copy(pop.noeudActuel.OffsetEnfants, idxInsertion + 1, pop.noeudActuel.OffsetEnfants, idxInsertion + 2, IdxPivot - idxInsertion);
          }
          // noeudActuel[idxInsertion] = eltAInserer
          Array.Copy(dataRemontee.elementRemonte, 0, pop.noeudActuel.data, 1 + TailleElement * idxInsertion, TailleElement);
          if (!pop.noeudActuel.IsFeuille)
          {
            // noeudActuel.Enfants[idxInsertion + 1] = noeudSuperieur
            pop.noeudActuel.Enfants[idxInsertion + 1] = dataRemontee.enfantPlus;
            pop.noeudActuel.OffsetEnfants[idxInsertion + 1] = dataRemontee.offsetEnfantPlus;
          }
        }
        else if (idxInsertion == IdxPivot + 1)
        {
          // eltRemonte = eltAInserer
          Array.Copy(dataRemontee.elementRemonte, 0, dataRemonteeSuivante.elementRemonte, 0, TailleElement);
          // copie de noeudActuel[idxPivot + 1 .. Ordre – 2] --> nouveauNoeud[0 .. Ordre – 3 – idxPivot] ' nb = Ordre – 2 – idxPivot
          Array.Copy(pop.noeudActuel.data, 1 + TailleElement * (IdxPivot + 1), nouveauNoeud.data, 1, TailleElement * (Ordre - 2 - IdxPivot));
          if (!pop.noeudActuel.IsFeuille)
          {
            // nouveauNoeud[0] = noeudSuperieur
            nouveauNoeud.Enfants[0] = dataRemontee.enfantPlus;
            nouveauNoeud.OffsetEnfants[0] = dataRemontee.offsetEnfantPlus;
            // copie de noeudActuel.Enfants[idxPivot + 2 .. Ordre – 1] --> nouveauNoeud.Enfants[1 .. Ordre – 2 – idxPivot]
            Array.Copy(pop.noeudActuel.Enfants, IdxPivot + 2, nouveauNoeud.Enfants, 1, Ordre - 2 - IdxPivot);
            Array.Copy(pop.noeudActuel.OffsetEnfants, IdxPivot + 2, nouveauNoeud.OffsetEnfants, 1, Ordre - 2 - IdxPivot);
          }
        }
        else
        {
          // eltRemonte = noeudActuel[idxPivot + 1]
          Array.Copy(pop.noeudActuel.data, 1 + TailleElement * (IdxPivot + 1), dataRemonteeSuivante.elementRemonte, 0, TailleElement);
          // copie de noeudActuel[idxPivot + 2 .. idxInsertion – 1] --> nouveauNoeud[0 .. idxInsertion – 3 – idxPivot] ' nb = idxInsertion – 2 – idxPivot
          Array.Copy(pop.noeudActuel.data, 1 + TailleElement * (IdxPivot + 2), nouveauNoeud.data, 1, TailleElement * (idxInsertion - 2 - IdxPivot));
          if (!pop.noeudActuel.IsFeuille)
          {
            // copie de noeudActuel.Enfants[idxPivot + 2 .. idxInsertion]  nouveauNoeud[0 .. idxInsertion – 2 – idxPivot]
            Array.Copy(pop.noeudActuel.Enfants, IdxPivot + 2, nouveauNoeud.Enfants, 0, idxInsertion - 1 - IdxPivot);
            Array.Copy(pop.noeudActuel.OffsetEnfants, IdxPivot + 2, nouveauNoeud.OffsetEnfants, 0, idxInsertion - 1 - IdxPivot);
          }
          // nouveauNoeud[idxInsertion – 2 – idxPivot] = eltAInserer
          Array.Copy(dataRemontee.elementRemonte, 0, nouveauNoeud.data, 1 + TailleElement * (idxInsertion - 2 - IdxPivot), TailleElement);
          if (!pop.noeudActuel.IsFeuille)
          {
            // nouveauNoeud.Enfants[idxInsertion – 1 – idxPivot] = noeudSuperieur
            nouveauNoeud.Enfants[idxInsertion - 1 - IdxPivot] = dataRemontee.enfantPlus;
            nouveauNoeud.OffsetEnfants[idxInsertion - 1 - IdxPivot] = dataRemontee.offsetEnfantPlus;
          }
          // copie de noeudActuel[idxInsertion .. Ordre − 2] --> nouveauNoeud[idxInsertion – 1 − idxPivot .. Ordre – 3 – idxPivot] ' nb = Ordre – 1 – idxInsertion
          Array.Copy(pop.noeudActuel.data, 1 + TailleElement * (idxInsertion), nouveauNoeud.data, 1 + TailleElement * (idxInsertion - 1 - IdxPivot), TailleElement * (Ordre - 1 - idxInsertion));
          if (!pop.noeudActuel.IsFeuille)
          {
            // copie de noeudActuel.Enfants[idxInsertion + 1 .. Ordre − 1] --> nouveauNoeud[idxInsertion – idxPivot .. Ordre – 2 − idxPivot]
            Array.Copy(pop.noeudActuel.Enfants, idxInsertion + 1, nouveauNoeud.Enfants, idxInsertion - IdxPivot, Ordre - 1 - idxInsertion);
            Array.Copy(pop.noeudActuel.OffsetEnfants, idxInsertion + 1, nouveauNoeud.OffsetEnfants, idxInsertion - IdxPivot, Ordre - 1 - idxInsertion);
          }
        }
        // noeudActuel.NbElt = idxPivot + 1
        pop.noeudActuel.NbElements = (byte)(IdxPivot + 1);
        // nouveauNoeud.NbElt = Ordre – 2 – idxPivot
        nouveauNoeud.NbElements = (byte)(Ordre - 2 - IdxPivot);
        DataRemontee tmpDataRemontee = dataRemontee;
        // eltAInserer = eltRemonte
        dataRemontee = dataRemonteeSuivante;
        // noeudSuperieur = nouveauNoeud
        dataRemonteeSuivante = tmpDataRemontee;
        pop.noeudActuel.dirty = true;
        nouveauNoeud.dirty = true;
      }
      // On n'a rencontré aucun noeud non saturé dans lequel insérer l'élément à remonter.
      // On crée alors une nouvelle racine avec ce seul élément, qui pointe sur l'ancienne racine et l'enfant supérieur de l'élément à remonter.
      CreeNouvelleRacine(dataRemontee);
      return InsertOrUpdateResult.Inserted;
    }
    private void CreeNouvelleRacine(DataRemontee dataRemontee)
    {
      int nouvelleProfondeur = Profondeur + 1;
      StockageNoeud nouvelleRacine = new StockageNoeud(Ordre, TailleElement, false);
      Array.Copy(dataRemontee.elementRemonte, 0, nouvelleRacine.data, 1, TailleElement);
      nouvelleRacine.Enfants[0] = Racine;
      nouvelleRacine.OffsetEnfants[0] = 0;
      nouvelleRacine.Enfants[1] = dataRemontee.enfantPlus;
      nouvelleRacine.OffsetEnfants[1] = dataRemontee.offsetEnfantPlus;
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
      Niveaux.Add(new StockageNiveau(nouvelleProfondeur, fi0, Ordre, TailleElement, false));
      Racine = nouvelleRacine;
      PileInsertion.Add(new DataInsertion());
    }
    // Recherche dichotomique d'un élément E dans une liste triée L de taille T
    // retourne false si E absent de L, true si E trouvé dans L
    // Si E trouvé dans L, idxPosition contient l'indice (0-based) de E dans L
    // Si E absent de L, idxPosition contient l'une des valeurs suivantes :
    //   si E < L[0]
    //     idxPosition vaut -1
    //   sinon
    //     idxPosition est l'indice du plus grand élément E' de L tel que E' < E
    private unsafe bool RechercheDichotomique(byte* pSituationNew, StockageNoeud actuel, out int idxPosition)
    {
      int idxInf = 0, idxSup = actuel.NbElements - 1;
      fixed (byte* pSituationData = &actuel.data[1])
      {
        for (; ; )
        {
          int idxPivot = (idxInf + idxSup) / 2;
          byte* pSituationPivot = pSituationData + idxPivot * TailleElement;
          int cmpResult = Comparateur.CompareSituations(pSituationNew, pSituationPivot);
          if (cmpResult == 0)
          {
            idxPosition = idxPivot;
            return true;
          }
          if (cmpResult < 0)
          {
            if (idxPivot == idxInf)
            {
              idxPosition = idxInf - 1;
              return false;
            }
            idxSup = idxPivot - 1;
          }
          else
          {
            if (idxPivot == idxSup)
            {
              idxPosition = idxSup;
              return false;
            }
            idxInf = idxPivot + 1;
          }
        }
      }
    }

    private StockageNoeud GetNoeudEnfant(StockageNoeud noeud, int profondeurNoeud, int idxEnfant)
    {
      if (noeud.Enfants[idxEnfant] == null)
      {
        noeud.Enfants[idxEnfant] = Niveaux[profondeurNoeud - 1].ChargeNoeud(noeud.OffsetEnfants[idxEnfant]);
      }
      return noeud.Enfants[idxEnfant];
    }

    public void Flush()
    {
      StockageNoeud noeud = Racine;
      Flush(Racine, Profondeur, 0);
    }

    private void Flush(StockageNoeud noeud, int profondeur, UInt32 offsetNoeud)
    {
      if (noeud.dirty)
      {
        Niveaux[profondeur].Flush(noeud, offsetNoeud);
      }
      if (!noeud.IsFeuille)
      {
        for (int idxEnfant = 0; idxEnfant <= noeud.NbElements; idxEnfant++)
        {
          Flush(noeud.Enfants[idxEnfant], profondeur - 1, noeud.OffsetEnfants[idxEnfant]);
        }
      }
    }
    public void Close()
    {
      for (int idxProfondeur = 0; idxProfondeur <= Profondeur; idxProfondeur++)
      {
        Niveaux[idxProfondeur].Close();
      }
    }
  }
}
