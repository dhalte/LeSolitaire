using LeSolitaireLogique;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserControls
{
  // Indique quelles opérations sont susceptibles de pouvoir être lancées
  [Flags]
  public enum enumOp
  {
    None = 0,
    Initialiser = 1,
    ConsoliderSolutions = 2,
    Rechercher = 4,
    ReglerNF = 8,
    ArrangerND = 16,
    Suspendre = 32,
  }
  internal class Common
  {
    public static SituationRaw ChargeSituationRaw(string description) { throw new NotImplementedException(); }
    public static Etendue CalculeEtendue(SituationRaw liste) { throw new NotImplementedException(); }
    internal static int Compare(bool[] pierres1, bool[] pierres2) { throw new NotImplementedException(); }
    public static void Convert(bool[] pierres, byte[] idxPierres) { throw new NotImplementedException(); }
    public static string Dump(byte[] idxPierres, Plateau plateau) { throw new NotImplementedException(); }
    internal static string ConvertFlags(byte idxSG) { throw new NotImplementedException(); }
  }
  public enum enumAccesData
  {
    EDdat,
    EDtmp,
    EFdat,
    EFtmp
  }
  public enum enumDirection
  {
    nord,
    est,
    sud,
    ouest
  }
  public enum enumSymetries
  {
    Id,
    rot90,
    rot180,
    rot270,
    hor,
    vert,
    premDiag,
    secondDiag
  }
  public class Pilote
  {
    public Pilote(FileInfo file) { }
    public int Nd { get; private set; }
    public int Nf { get; private set; }
    public long IdxReprise;
    public string DBname;
    // C'est cet objet qui contient l'étendue du plateau
    public SituationRaw PlateauRaw;
    public List<Solution> Solutions;
    public List<PreSolution> PreSolutions;
    public Pilote() { }
  }
  internal class Plateau
  {
    public Plateau(SituationRaw situation) { }
    public Etendue Etendue;
    // tableau de la taille du rectangle englobant
    public bool[] PresenceCase;
    // liste des indices des cases du plateau
    public byte[] Cases;
    public int NbCasesPlateau => Cases.Length;
    public List<(byte idxOrigine, byte idxVoisin, byte idxDestination)> MouvementsPossibles;
    // Chaque élément de cette liste est un tableau de Etendue.Largeur * Etendue.Hauteur indirections.
    // Quand on veut calculer l'image d'une situation donnée par la liste des indices de ses pierres, 
    // on utilise chacun de ces indices dans un de ces tableaux pour calculer l'indice image.
    public List<byte[]> Symetries;
    public int NbSymetries => Symetries.Count;
    public bool Contains(int idxCase) => 0 <= idxCase && idxCase < Etendue.NbCasesRectangleEnglobant && PresenceCase[idxCase];
    public bool Contains(int x, int y) => 0 <= x && x < Etendue.Largeur && 0 <= y && y < Etendue.Hauteur && Contains(Etendue.FromXY(x, y));
  }
  public class Solution
  {
    public SituationRaw SituationInitialeRaw;
    public Situation SituationInitiale;
    public bool Complete;
    public List<SolutionMouvement> Mouvements = new List<SolutionMouvement>();
  }
  public class SolutionMouvement
  {
    public readonly byte IdxDepart;
    public readonly byte IdxSaut;

    public SolutionMouvement(byte idxPierre, byte idxSaut)
    {
      IdxDepart = idxPierre;
      IdxSaut = idxSaut;
    }
    public byte IdxArrivee(Etendue etendue)
    {
      return etendue.IdxArrivee(IdxDepart, IdxSaut);
    }
  }
  internal class Logique
  {
    public void LanceInitialiser(string filenameFicheDeJeu) { }
    public enumState State { get; private set; } = enumState.stopped;
    public enumOp EnumOp { get; private set; }
    public LogiqueConfiguration Config;
    public SituationStock SituationStock;
    public bool LowMemory;
    public Logique(Feedback parent) { }
    public enumOp Verifie(string filenameFicheDeJeu) { throw new NotImplementedException(); }
    public void StoppeBgTask() { }
    public void LanceReglerNF(int nf) { }
    public void LanceRecherche() { }
    public void LanceConsolider() { }
  }
  public class LogiqueConfiguration
  {
    // Le fichier fourni, ne contient que le plateau initial
    public FileInfo FicheDeJeu;
    // Le répertoire qui porte le nom de FicheDeJeu et dans lequel on stocke les données de la recherche
    public DirectoryInfo RacineData;
    // Le fichier pilote dans le répertoire
    public FileInfo FichierPilote;
    // Le contenu du fichier FichierPilote
    public Pilote Pilote;
    // La taille des situations dans le fichier ED
    public int TailleSituationsND;
    // La taille des situations dans le fichier EF
    public int TailleSituationsNF;
    public FileStream OpenData(enumAccesData accesData) { throw new NotImplementedException(); }
    public void ReplaceData(enumAccesData accesData) { throw new NotImplementedException(); }
    public bool Exist(enumAccesData accesData) { throw new NotImplementedException(); }
    public long FileSize(enumAccesData accesData) { throw new NotImplementedException(); }
    public void Supprime(enumAccesData accesData) { }
  }
  public class Etendue
  {
    public Etendue(int largeur, int hauteur)
    {
      this.Largeur = largeur;
      this.Hauteur = hauteur;
      xCentre = Largeur / 2;
      yCentre = Hauteur / 2;
      NbCasesRectangleEnglobant = Largeur * Hauteur;
    }
    public readonly int Largeur;
    public readonly int Hauteur;
    public readonly int NbCasesRectangleEnglobant;
    // Celui utilisé pour convertir en coordonnées de part et d'autre de l'origine
    // Ne pas utiliser pour le calcul des symétries (quoique pour les plateaux classique et français, les deux coïncident)
    public readonly int xCentre;
    public readonly int yCentre;

    public bool Contains(int idxCase) => 0 <= idxCase && idxCase < NbCasesRectangleEnglobant;
    public bool Contains(int x, int y) => 0 <= x && x < Largeur && 0 <= y && y < Hauteur;

    //Indice case dans le rectangle englobant, à partir de 0.
    //Les coordonnées minimales sont 0
    public byte FromXY(int x, int y) => (byte)(x + Largeur * y);
    public byte FromXY((int x, int y) coordonnees) => (byte)(coordonnees.x + Largeur * coordonnees.y);

    //Indice case dans le rectangle minimal contenant le plateau, à partir de 0
    //Les coordonnées sont centrées
    public byte FromXYCentre(int x, int y) => (byte)((x + xCentre) + Largeur * (y + yCentre));

    // coordonnées case dans le rectangle englobant, les coordonnées minimales sont 0
    public (int x, int y) FromByte(byte i) => (i % Largeur, i / Largeur);

    // Fournir des coordonnées centrées autour de l'origine, pour une lecture plus facile des solutions.
    public (int x, int y) Centrer(int x, int y) => (x - xCentre, y - yCentre);
    public (int x, int y) Centrer((int x, int y) c) => (c.x - xCentre, c.y - yCentre);

    public byte IdxArrivee(byte idxDepart, byte idxSaut)
    {
      (int x, int y) depart = FromByte(idxDepart);
      (int x, int y) saut = FromByte(idxSaut);
      return FromXY(2 * saut.x - depart.x, 2 * saut.y - depart.y);
    }
  }

  internal class SituationEtude
  {
    public SituationEtude(Plateau plateau) { }
    public bool[] Pierres;
    public void ChargeSituation(byte[] pierres) { }
    public void ChargeSituation(Situation situation) { }
    public void ChargeSituationRaw(SituationRaw situationRaw) { }
    public void EffectueMouvement((byte idxOrigine, byte idxVoisin, byte idxDestination) mvt) { }
  }
  public enum enumState
  {
    running,
    stopping,
    stopped
  }
  public class SituationRaw : List<(int x, int y, bool pierre)> 
  { 
  
  }
  public class Situation
  {
    public Situation(Etendue etendue, SituationRaw plateauRaw) { }
    public Situation(bool[] pierresImages) { }
  }
  public class SituationStock 
  { 

  }
  public class PreSolution
  {
    public long IdxSD;
    public List<int> IdxSIlist = new List<int>();
    public List<SolutionMouvement> Mouvements = new List<SolutionMouvement>();
  }
}
