using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using BTree;

namespace LeSolitaireLogique.Services
{
  internal class Plateau : IComparateurSituations
  {
    private const string NomFichierPilote = "pilote.xml";

    // Les 5 octets décrivant l'état des cases d'une situation
    internal const int TailleDescriptionSituation = 5;
    // L'octet supplémentaire décrivant les situations initiales pouvant mener à la situation décrite
    internal const int TailleDescriptionSituationEtMasque = TailleDescriptionSituation + 1;
    internal readonly int NbCases;
    // Les cases sont repérées par leurs coordonnées.
    // Elles sont cadrées de manière que les cases de plus petite abscisse aient une abscisse de 0,
    // et celles de plus petite ordonnée aient une ordonnées de 0.
    // TailleRectangleEnglobant donne la taille du plus petit rectangle encadrant ces cases.
    // Donc la plus grande abscisse est Width-1, la plus grande ordonnée est Height-1
    private readonly (int Width, int Height) TailleRectangleEnglobant;
    // Tableau permettant de connaitre l'indice d'une case du plateau en fonction de ses coordonnées
    // Les cases du plateau sont indicées à partir de 0.
    // Les cases du rectangle englobant qui ne sont pas du plateau ont un indice négatif.
    private readonly int[,] IndicesCasesInRectangleEnglobant;
    // Tableau permettant de connaitre les coordonnées d'une case sachant son indice
    private readonly (int x, int y)[] CoordonneesCases;
    internal readonly List<Mouvement> MouvementsPossibles;

    // Indices et masques à utiliser pour manipuler les byte[] représentant l'état des cases d'un plateau
    // Ne dépend pas des coordonnées de ces cases
    private readonly (int indice, byte masque, byte masqueComplementaire)[] IndicesEtMasquesCases;
    // Liste des rotations et symétries conservant globalement le plateau.
    // On n'inclue pas l'identité dans cette liste.
    private readonly List<int[]> SymetriesPlateau;
    // Liste des inverses de ces rotations et symétries, classées dans le même ordre qu'elles
    private readonly List<int[]> InversesSymetriesPlateau;
    // Tableau permettant de connaitre un mouvement originel sur une situation sit
    // connaissant la symétrie qui lui a été appliquée pour obtenir une situation normalisée
    // et le mouvement appliqué à cette situation normalisée.
    private readonly int[][] MouvementsOriginels;
    // Ce BufferSituation est un tableau de (1 + nb symétries possibles) situations
    // (on n'inclue pas le dernier byte, i.e. le flag des situations initiales associées)
    // La première situation est utilisée pour calculer toutes les situations équivalentes par symétrie, 
    // chacune placée dans les autres cases de BufferSituation.
    private readonly byte[][] BufferSituation;
    // buffer tampon pour calculer le dual d'une situation
    internal byte[] situationDuale;
    // Variable d'état du plateau
    internal bool Complet;
    internal EnumReprise Reprise = EnumReprise.None;
    internal byte[] SituationReprise;
    // Liste des solutions déjà déterminées (complètes ou partielles)
    private readonly List<Solution> Solutions = new List<Solution>();

    private Plateau(List<(int x, int y)> cases)
    {
      #region Premières vérifications
      if (cases == null)
      {
        throw new ArgumentNullException("cases");
      }
      NbCases = cases.Count;
      if (NbCases == 0)
      {
        throw new ArgumentException("Aucune case de plateau n'a été trouvée dans la description");
      }
      // On code l'état des cases sur 5 octets, 40=5*8
      if (NbCases > 8 * TailleDescriptionSituation)
      {
        throw new ArgumentException("Le plateau contient trop de cases");
      }
      int xMin = int.MaxValue, xMax = int.MinValue;
      int yMin = int.MaxValue, yMax = int.MinValue;
      foreach (var (x, y) in cases)
      {
        if (xMin > x) xMin = x;
        if (xMax < x) xMax = x;
        if (yMin > y) yMin = y;
        if (yMax < y) yMax = y;
      }
      if (xMin != 0)
      {
        throw new ArgumentException("Les cases du plateau les plus à gauche doivent avoir une abscisse à 0");
      }
      if (yMin != 0)
      {
        // On oriente l'axe des ordonnées de haut en bas
        throw new ArgumentException("Les cases du plateau les plus hautes doivent avoir une ordonnée à 0");
      }
      CoordonneesCases = new (int x, int y)[NbCases];
      for (int idx = 0; idx < NbCases; idx++)
      {
        CoordonneesCases[idx] = cases[idx];
      }
      // Tri des cases du plateau de gauche à droite depuis le haut vers le bas
      Array.Sort(CoordonneesCases, ((int x, int y) c1, (int x, int y) c2) => c1.y == c2.y ? c1.x.CompareTo(c2.x) : c1.y.CompareTo(c2.y));
      #endregion Premières vérifications

      #region Construction d'un tableau permettant de connaitre l'indice d'une case du plateau en fonction de ses coordonnées
      TailleRectangleEnglobant = (xMax + 1, yMax + 1);
      IndicesCasesInRectangleEnglobant = new int[TailleRectangleEnglobant.Width, TailleRectangleEnglobant.Height];
      // Initialisation de toutes les cases de ce tableau à -1
      for (int y = 0; y <= yMax; y++)
      {
        for (int x = 0; x <= xMax; x++)
        {
          IndicesCasesInRectangleEnglobant[x, y] = -1;
        }
      }
      // Attribution aux cases représentant une case du plateau d'un indice croissant à partir de 0
      for (int idx = 0; idx < NbCases; idx++)
      {
        (int x, int y) = CoordonneesCases[idx];
        IndicesCasesInRectangleEnglobant[x, y] = idx;
      }
      #endregion Construction d'un tableau permettant de connaitre l'indice d'une case du plateau en fonction de ses coordonnées

      #region Etablissement des symétries qui conservent globalement le plateau
      SymetriesPlateau = new List<int[]>();
      InversesSymetriesPlateau = new List<int[]>();
      bool bCarre = xMax - xMin == yMax - yMin;
      foreach (enumSymetries symetrie in Enum.GetValues(typeof(enumSymetries)))
      {
        Matrice2x3 matrice = null;
        switch (symetrie)
        {
          case enumSymetries.rot90:
            if (bCarre)
            {
              matrice = new Matrice2x3((0, -1, xMin + yMax), (1, 0, -xMin + yMin));
            }
            break;
          case enumSymetries.rot180:
            matrice = new Matrice2x3((-1, 0, xMin + xMax), (0, -1, yMin + yMax));
            break;
          case enumSymetries.rot270:
            if (bCarre)
            {
              matrice = new Matrice2x3((0, 1, xMin - yMin), (-1, 0, xMin + yMax));
            }
            break;
          case enumSymetries.hor:
            matrice = new Matrice2x3((1, 0, 0), (0, -1, yMin + yMax));
            break;
          case enumSymetries.vert:
            matrice = new Matrice2x3((-1, 0, xMin + xMax), (0, 1, 0));
            break;
          case enumSymetries.premDiag:
            if (bCarre)
            {
              matrice = new Matrice2x3((0, 1, xMin - yMin), (1, 0, -xMin + yMin));
            }
            break;
          case enumSymetries.secondDiag:
            if (bCarre)
            {
              matrice = new Matrice2x3((0, -1, xMin + yMax), (-1, 0, xMin + yMax));
            }
            break;
        }
        if (matrice != null)
        {
          int[] indirection = new int[NbCases];
          int[] inverseIndirection = new int[NbCases];
          if (CalculeSymetriePlateau(matrice, indirection, inverseIndirection))
          {
            SymetriesPlateau.Add(indirection);
            InversesSymetriesPlateau.Add(inverseIndirection);
          }
        }
      }
      #endregion Etablissement des symétries qui conservent globalement le plateau

      #region Recherche des mouvements possibles
      MouvementsPossibles = new List<Mouvement>();
      for (int yc1 = 0; yc1 <= yMax; yc1++)
      {
        for (int xc1 = 0; xc1 <= xMax; xc1++)
        {
          if (IndicesCasesInRectangleEnglobant[xc1, yc1] < 0)
          {
            continue;
          }
          int xc2, yc2, xc3, yc3;
          // vers le nord
          xc2 = xc1; yc2 = yc1 - 1; xc3 = xc2; yc3 = yc2 - 1;
          if (!(yc2 < 0 || yc3 < 0 || IndicesCasesInRectangleEnglobant[xc2, yc2] < 0 || IndicesCasesInRectangleEnglobant[xc3, yc3] < 0))
          {
            MouvementsPossibles.Add(new Mouvement(IndicesCasesInRectangleEnglobant[xc1, yc1], IndicesCasesInRectangleEnglobant[xc2, yc2], IndicesCasesInRectangleEnglobant[xc3, yc3]));
          }
          // vers l'est
          xc2 = xc1 + 1; yc2 = yc1; xc3 = xc2 + 1; yc3 = yc2;
          if (!(xc2 > xMax || xc3 > xMax || IndicesCasesInRectangleEnglobant[xc2, yc2] < 0 || IndicesCasesInRectangleEnglobant[xc3, yc3] < 0))
          {
            MouvementsPossibles.Add(new Mouvement(IndicesCasesInRectangleEnglobant[xc1, yc1], IndicesCasesInRectangleEnglobant[xc2, yc2], IndicesCasesInRectangleEnglobant[xc3, yc3]));
          }
          // vers le sud
          xc2 = xc1; yc2 = yc1 + 1; xc3 = xc2; yc3 = yc2 + 1;
          if (!(yc2 > yMax || yc3 > yMax || IndicesCasesInRectangleEnglobant[xc2, yc2] < 0 || IndicesCasesInRectangleEnglobant[xc3, yc3] < 0))
          {
            MouvementsPossibles.Add(new Mouvement(IndicesCasesInRectangleEnglobant[xc1, yc1], IndicesCasesInRectangleEnglobant[xc2, yc2], IndicesCasesInRectangleEnglobant[xc3, yc3]));
          }
          // vers l'ouest
          xc2 = xc1 - 1; yc2 = yc1; xc3 = xc2 - 1; yc3 = yc2;
          if (!(xc2 < 0 || xc3 < 0 || IndicesCasesInRectangleEnglobant[xc2, yc2] < 0 || IndicesCasesInRectangleEnglobant[xc3, yc3] < 0))
          {
            MouvementsPossibles.Add(new Mouvement(IndicesCasesInRectangleEnglobant[xc1, yc1], IndicesCasesInRectangleEnglobant[xc2, yc2], IndicesCasesInRectangleEnglobant[xc3, yc3]));
          }
        }
      }
      // Etablissement des indices et masques à utiliser pour manipuler les byte[] représentant l'état des cases d'un plateau
      // Voir documentation : pour garder la comparaison efficace et rendre son résultat "naturel", les cases sont codées à partir du poids fort dans chaque octet.
      IndicesEtMasquesCases = new (int indice, byte masque, byte masqueComplementaire)[NbCases];
      for (int i = 0; i < NbCases; i++)
      {
        IndicesEtMasquesCases[i] = (i / 8, (byte)(1 << (7 - (i & 7))), (byte)~(1 << (7 - (i & 7))));
      }
      #endregion Recherche des mouvements possibles

      #region Mouvements originels sachant une symétrie et un mouvement
      MouvementsOriginels = new int[SymetriesPlateau.Count][];
      for (int idxSym = 0; idxSym < SymetriesPlateau.Count; idxSym++)
      {
        int[] sym = InversesSymetriesPlateau[idxSym];
        int[] mvtOri = new int[MouvementsPossibles.Count];
        MouvementsOriginels[idxSym] = mvtOri;
        for (int idxMvt = 0; idxMvt < MouvementsPossibles.Count; idxMvt++)
        {
          Mouvement m = MouvementsPossibles[idxMvt];
          int c1 = sym[m.c1];
          int c2 = sym[m.c2];
          int c3 = sym[m.c3];
          int idxOriMvt = -1;
          for (int idxMvt2 = 0; idxMvt2 < MouvementsPossibles.Count; idxMvt2++)
          {
            Mouvement m2 = MouvementsPossibles[idxMvt2];
            if (c1 == m2.c1 && c2 == m2.c2 && c3 == m2.c3)
            {
              idxOriMvt = idxMvt2;
              break;
            }
          }
          mvtOri[idxMvt] = idxOriMvt;
        }
      }
      #endregion Mouvements originels sachant une symétrie et un mouvement

      #region Dernières allocations de buffers
      BufferSituation = new byte[1 + SymetriesPlateau.Count][];
      for (int idxSym = 0; idxSym <= SymetriesPlateau.Count; idxSym++)
      {
        BufferSituation[idxSym] = new byte[TailleDescriptionSituation];
      }
      situationDuale = new byte[TailleDescriptionSituationEtMasque];
      #endregion Dernières allocations de buffers
    }

    internal void EnregistrerSolutionPartielle(DirectoryInfo repertoire, byte[] situation, IEnumerable<int> mvts)
    {
      string fichierPilote = Path.Combine(repertoire.FullName, NomFichierPilote);
      if (!File.Exists(fichierPilote))
      {
        throw new ApplicationException($"impossible de trouver le fichier pilote {fichierPilote}");
      }
      XmlDocument xDoc = new XmlDocument();
      // Peut déclencher une erreur si syntaxe incorrecte
      xDoc.Load(fichierPilote);
      XmlElement xRoot = xDoc.DocumentElement;
      XmlElement xSolution = xDoc.CreateElement("Solution");
      xRoot.AppendChild(xSolution);
      xSolution.SetAttribute("situation", ToString(situation));
      foreach (int mvt in mvts)
      {
        XmlElement xMvt = xDoc.CreateElement("Mvt");
        xSolution.AppendChild(xMvt);
        xMvt.SetAttribute("idx", mvt.ToString());
      }
      xDoc.Save(fichierPilote);
    }

    internal void EnregistrerSuspension(DirectoryInfo repertoire, byte[] situation, EnumReprise reprise)
    {
      string fichierPilote = Path.Combine(repertoire.FullName, NomFichierPilote);
      if (!File.Exists(fichierPilote))
      {
        throw new ApplicationException($"impossible de trouver le fichier pilote {fichierPilote}");
      }
      XmlDocument xDoc = new XmlDocument();
      // Peut déclencher une erreur si syntaxe incorrecte
      xDoc.Load(fichierPilote);
      XmlElement xRoot = xDoc.DocumentElement;
      XmlElement xEtat = xRoot["Etat"];
      if (xEtat == null)
      {
        throw new ApplicationException($"Fichier pilote incorrect, description état introuvable");
      }
      xEtat.SetAttribute("situation", ToString(situation));
      xEtat.SetAttribute("reprise", reprise == EnumReprise.EnLargeur ? "largeur" : "profondeur");
      xDoc.Save(fichierPilote);
    }


    // Solutions complètes ou partielles confondues
    internal byte CalculeSolutionsConnues()
    {
      byte flag = 0;
      foreach (var solution in Solutions)
      {
        flag |= solution.Situation[TailleDescriptionSituationEtMasque - 1];
      }
      return flag;
    }
    internal byte CalculeSolutionsCompletesConnues()
    {
      byte flag = 0;
      foreach (var solution in Solutions)
      {
        if (solution.Mouvements.Count == NbCases - 2)
        {
          flag |= solution.Situation[TailleDescriptionSituationEtMasque - 1];
        }
      }
      return flag;
    }
    internal class EnumerationNouvellesSituationsNormalisees
    {
      private readonly Plateau Plateau;
      private int idxMvt = 0;
      private readonly byte[] Situation;
      internal int idxMvtToNouvelleSituation;
      internal byte[] NouvelleSituation;
      internal EnumerationNouvellesSituationsNormalisees(Plateau plateau, byte[] situation)
      {
        Plateau = plateau;
        Situation = situation;
        NouvelleSituation = new byte[situation.Length];
      }
      internal bool Next()
      {
        for (; idxMvt < Plateau.MouvementsPossibles.Count; idxMvt++)
        {
          if (Plateau.MouvementAutorise(Situation, idxMvt))
          {
            Plateau.MouvementEffectue(Situation, idxMvt, NouvelleSituation);
            Plateau.Normalise(NouvelleSituation, out int idxSymetrieAppliquee);
            idxMvtToNouvelleSituation = idxMvt++;
            return true;
          }
        }
        return false;
      }
    }

    // La situation passée en paramètre comporte ou non le flag
    // Elle est remplacée par la plus petite siuation équivalente, le flag, s'il est présent, n'est pas touché.
    // L'indice de la symétrie appliquée est -1 si la situation est déjà la plus petite, sinon il est l'indice de la symétrie utilisée.
    internal unsafe void Normalise(byte[] situation, out int idxSymetrieAppliquee)
    {
      // Copie sans le flag des états des cases de la situation dans le premier buffer
      Array.Copy(situation, BufferSituation[0], TailleDescriptionSituation);

      // Calcul de toutes les situations équivalentes et ajout aux buffers de BufferSituation
      for (int idxSymetrie = 0; idxSymetrie < SymetriesPlateau.Count; idxSymetrie++)
      {
        int[] symetrie = SymetriesPlateau[idxSymetrie];
        for (int idxCase = 0; idxCase < NbCases; idxCase++)
        {
          if (CaseOccupee(situation, idxCase))
          {
            PlacePierre(BufferSituation[1 + idxSymetrie], symetrie[idxCase]);
          }
          else
          {
            EnlevePierre(BufferSituation[1 + idxSymetrie], symetrie[idxCase]);
          }
        }
      }
      // Comparaison des situations équivalentes pour déterminer la plus petite d'entre elles
      int idxSituationNormalisee = 0;
      for (int idxSituation = 1; idxSituation <= SymetriesPlateau.Count; idxSituation++)
      {
        int n = CompareSituations(BufferSituation[idxSituation], BufferSituation[idxSituationNormalisee]);
        if (n < 0)
        {
          idxSituationNormalisee = idxSituation;
        }
      }
      if (idxSituationNormalisee > 0)
      {
        Array.Copy(BufferSituation[idxSituationNormalisee], situation, TailleDescriptionSituation);
      }
      idxSymetrieAppliquee = idxSituationNormalisee - 1;
    }

    private void MouvementEffectue(byte[] situation, int idxMvt, byte[] nouvelleSituation)
    {
      Array.Copy(situation, nouvelleSituation, TailleDescriptionSituationEtMasque);
      Mouvement mvt = MouvementsPossibles[idxMvt];
      EnlevePierre(nouvelleSituation, mvt.c1);
      EnlevePierre(nouvelleSituation, mvt.c2);
      PlacePierre(nouvelleSituation, mvt.c3);
    }
    internal void MouvementDualEffectue(byte[] situation, int idxMvt, byte[] situationPrecedente)
    {
      Array.Copy(situation, situationPrecedente, TailleDescriptionSituationEtMasque);
      Mouvement mvt = MouvementsPossibles[idxMvt];
      PlacePierre(situationPrecedente, mvt.c1);
      PlacePierre(situationPrecedente, mvt.c2);
      EnlevePierre(situationPrecedente, mvt.c3);
    }

    private bool MouvementAutorise(byte[] situation, int idxMvt)
    {
      Mouvement mvt = MouvementsPossibles[idxMvt];
      return CaseOccupee(situation, mvt.c1) && CaseOccupee(situation, mvt.c2) && !CaseOccupee(situation, mvt.c3);
    }
    internal bool MouvementDualAutorise(byte[] situation, int idxMvt)
    {
      Mouvement mvt = MouvementsPossibles[idxMvt];
      return !CaseOccupee(situation, mvt.c1) && !CaseOccupee(situation, mvt.c2) && CaseOccupee(situation, mvt.c3);
    }

    private bool CalculeSymetriePlateau(Matrice2x3 matrice, int[] indirection, int[] inverseIndirection)
    {
      bool bOK = true;
      int idxCase = 0;
      for (int y1 = 0; y1 < TailleRectangleEnglobant.Height; y1++)
      {
        for (int x1 = 0; x1 < TailleRectangleEnglobant.Width; x1++)
        {
          if (IndicesCasesInRectangleEnglobant[x1, y1] < 0)
          {
            continue;
          }
          int x2 = matrice.m[0, 0] * x1 + matrice.m[0, 1] * y1 + matrice.m[0, 2];
          int y2 = matrice.m[1, 0] * x1 + matrice.m[1, 1] * y1 + matrice.m[1, 2];
          if (IndicesCasesInRectangleEnglobant[x2, y2] < 0)
          {
            bOK = false;
            break;
          }
          indirection[idxCase] = IndicesCasesInRectangleEnglobant[x2, y2];
          inverseIndirection[IndicesCasesInRectangleEnglobant[x2, y2]] = idxCase;
          idxCase++;
        }
      }
      return bOK;
    }

    internal unsafe bool CaseOccupee(byte[] P, int i)
    {
      return (P[IndicesEtMasquesCases[i].indice] & IndicesEtMasquesCases[i].masque) != 0;
    }
    internal unsafe bool CaseOccupee(byte* P, int i)
    {
      return (P[IndicesEtMasquesCases[i].indice] & IndicesEtMasquesCases[i].masque) != 0;
    }
    internal unsafe void PlacePierre(byte* P, int i)
    {
      P[IndicesEtMasquesCases[i].indice] |= IndicesEtMasquesCases[i].masque;
    }
    internal void PlacePierre(byte[] P, int i)
    {
      P[IndicesEtMasquesCases[i].indice] |= IndicesEtMasquesCases[i].masque;
    }
    internal unsafe void EnlevePierre(byte* P, int i)
    {
      P[IndicesEtMasquesCases[i].indice] &= IndicesEtMasquesCases[i].masqueComplementaire;
    }
    internal void EnlevePierre(byte[] P, int i)
    {
      P[IndicesEtMasquesCases[i].indice] &= IndicesEtMasquesCases[i].masqueComplementaire;
    }
    internal int NbPierres(byte[] situation)
    {
      int nbPierres = 0;
      for (int idxCase = 0; idxCase < NbCases; idxCase++)
      {
        if (PierrePresente(situation, idxCase))
        {
          nbPierres++;
        }
      }
      return nbPierres;
    }

    private bool PierrePresente(byte[] p, int i)
    {
      byte f = IndicesEtMasquesCases[i].masque;
      return ((byte)(p[IndicesEtMasquesCases[i].indice] & f)) == f;
    }
    public unsafe int CompareSituations(byte* p1, byte* p2)
    {
      for (int i = 0; i < TailleDescriptionSituation; i++)
      {
        int c = p1[i].CompareTo(p2[i]);
        if (c != 0)
        {
          return c;
        }
      }
      return 0;
    }
    public unsafe int CompareSituations(byte[] p1, byte* p2)
    {
      for (int i = 0; i < TailleDescriptionSituation; i++)
      {
        int c = p1[i].CompareTo(p2[i]);
        if (c != 0)
        {
          return c;
        }
      }
      return 0;
    }
    public int CompareSituations(byte[] p1, byte[] p2)
    {
      for (int i = 0; i < TailleDescriptionSituation; i++)
      {
        int c = p1[i].CompareTo(p2[i]);
        if (c != 0)
        {
          return c;
        }
      }
      return 0;
    }
    public unsafe bool MajSituation(byte* pSituationNew, byte* pSituationExistante)
    {
      byte* bNew = &pSituationNew[TailleDescriptionSituationEtMasque - 1];
      byte* bOld = &pSituationExistante[TailleDescriptionSituationEtMasque - 1];
      byte bMaj = (byte)(*bOld | *bNew);
      if (*bOld == bMaj)
      {
        return false;
      }
      *bOld = bMaj;
      *bNew = bMaj;
      return true;
    }
    internal static Plateau DecodeDescription(string description)
    {
      if (string.IsNullOrEmpty(description))
      {
        throw new ArgumentNullException("description");
      }
      List<(int x, int y)> cases = new List<(int x, int y)>();
      int x, y, xMin, l;
      bool ligneVide;
      l = description.Length;
      x = y = 0;
      xMin = int.MaxValue;
      ligneVide = true;
      for (int i = 0; i < l; i++)
      {
        char c = description[i];
        switch (c)
        {
          case '\r':
          case '\n':
            if (!ligneVide)
            {
              y++;
            }
            x = 0;
            ligneVide = true;
            break;
          case 'x':
          case 'X':
            if (xMin > x) xMin = x;
            cases.Add((x++, y));
            ligneVide = false;
            break;
          default:
            x++;
            break;
        }
      }
      if (xMin > 0)
      {
        for (int i = 0; i < cases.Count; i++)
        {
          cases[i] = (cases[i].x - xMin, cases[i].y);
        }
      }
      return new Plateau(cases);
    }

    internal List<byte[]> CalculeSituationsInitiales()
    {
      byte[] situation = new byte[TailleDescriptionSituation];
      List<byte[]> result = new List<byte[]>();
      // Il faut gérer prudemment les derniers bits qui ne sont pas représentatifs d'une case, puisqu'il y a moins de cases que 5*8
      // On vide le plateau
      for (int idxByte = 0; idxByte < TailleDescriptionSituation; idxByte++)
      {
        situation[idxByte] = 0;
      }
      // On le remplit de pierres
      for (int idxCase = 0; idxCase < NbCases; idxCase++)
      {
        PlacePierre(situation, idxCase);
      }
      int idxPierrePrev = -1;
      // On va successivement enlever chaque pierre d'un plateau complet
      for (int idxPierre = 0; idxPierre < NbCases; idxPierre++)
      {
        // On remet la pierre précédemment enlevée
        if (idxPierrePrev >= 0)
        {
          PlacePierre(situation, idxPierrePrev);
        }
        // On enlève exactement une pierre
        EnlevePierre(situation, idxPierre);
        idxPierrePrev = idxPierre;
        Array.Copy(situation, BufferSituation[0], TailleDescriptionSituation);
        Normalise(BufferSituation[0], out int idxSymetrieAppliquee);

        // Test si cette situation de référence est nouvelle dans la liste des situations initiales
        bool bExists = false;
        foreach (byte[] situationInitiale in result)
        {
          if (CompareSituations(situationInitiale, BufferSituation[0]) == 0)
          {
            bExists = true;
            break;
          }
        }
        if (!bExists)
        {
          // Ajout de la nouvelle situation de référence, avec son champ de flags l'identifiant
          byte[] nvlSituation = new byte[TailleDescriptionSituationEtMasque];
          Array.Copy(BufferSituation[0], nvlSituation, TailleDescriptionSituation);
          nvlSituation[TailleDescriptionSituationEtMasque - 1] = (byte)(1 << result.Count);
          result.Add(nvlSituation);
        }
      }

      if (result.Count > 8)
      {
        throw new ApplicationException("Trop de situations initiales différentes");
      }
#if DEBUG
      for (int idxResult = 0; idxResult < result.Count; idxResult++)
      {
        byte[] res = result[idxResult];
        Debug.Print($"situation initiale {idxResult}, flag 0x{res[TailleDescriptionSituationEtMasque - 1]:X2}");
        Debug.Print(Dump(res));
      }
#endif  
      return result;
    }

    public unsafe string Dump(byte[] situation)
    {
      fixed (byte* pSituation = situation)
      {
        return Dump(pSituation);
      }
    }
    public unsafe string Dump(byte* situation)
    {
      char[,] result = new char[TailleRectangleEnglobant.Width, TailleRectangleEnglobant.Height];
      for (int y = 0; y < TailleRectangleEnglobant.Height; y++)
      {
        for (int x = 0; x < TailleRectangleEnglobant.Width; x++)
        {
          result[x, y] = ' ';
        }
      }
      for (int idxCase = 0; idxCase < NbCases; idxCase++)
      {
        (int x, int y) = CoordonneesCases[idxCase];
        if (CaseOccupee(situation, idxCase))
        {
          result[x, y] = 'x';
        }
        else
        {
          result[x, y] = 'o';
        }
      }
      StringBuilder sb = new StringBuilder();
      for (int y = 0; y < TailleRectangleEnglobant.Height; y++)
      {
        for (int x = 0; x < TailleRectangleEnglobant.Width; x++)
        {
          sb.Append(result[x, y]).Append('\t');
        }
        sb.AppendLine();
      }
      return sb.ToString();
    }

    internal void InitialiseFichierPilote(DirectoryInfo repertoire)
    {
      XmlDocument xDoc = new XmlDocument();
      xDoc.AppendChild(xDoc.CreateXmlDeclaration("1.0", null, null));
      XmlElement root = xDoc.CreateElement("LeSolitaire");
      xDoc.AppendChild(root);
      XmlElement plateau = xDoc.CreateElement("Plateau");
      root.AppendChild(plateau);
      for (int idxcase = 0; idxcase < NbCases; idxcase++)
      {
        (int x, int y) = CoordonneesCases[idxcase];
        XmlElement xCase = xDoc.CreateElement("Case");
        plateau.AppendChild(xCase);
        xCase.SetAttribute("x", x.ToString());
        xCase.SetAttribute("y", y.ToString());
      }
      XmlElement etat = xDoc.CreateElement("Etat");
      root.AppendChild(etat);
      etat.SetAttribute("complet", "false");
      etat.SetAttribute("reprise", "");
      etat.SetAttribute("situation", "");
      xDoc.Save(Path.Combine(repertoire.FullName, NomFichierPilote));
    }

    internal static Plateau ChargerFichierPilote(DirectoryInfo repertoire)
    {
      string fichierPilote = Path.Combine(repertoire.FullName, NomFichierPilote);
      if (!File.Exists(fichierPilote))
      {
        throw new ApplicationException($"impossible de trouver le fichier pilote {fichierPilote}");
      }
      XmlDocument xDoc = new XmlDocument();
      // Peut déclencher une erreur si syntaxe incorrecte
      xDoc.Load(fichierPilote);
      XmlElement xRoot = xDoc.DocumentElement;
      XmlElement xPlateau = xRoot["Plateau"];
      if (xPlateau == null)
      {
        throw new ApplicationException($"Fichier pilote incorrect, description plateau introuvable");
      }
      List<(int x, int y)> cases = new List<(int x, int y)>();
      foreach (XmlElement xCase in xPlateau.SelectNodes("Case"))
      {
        string sx = xCase.GetAttribute("x"), sy = xCase.GetAttribute("y");
        if (!int.TryParse(sx, out int x) || !int.TryParse(sy, out int y))
        {
          throw new ApplicationException($"Fichier pilote incorrect, coordonnées non ou mal définies : {xCase.OuterXml}");
        }
        cases.Add((x, y));
      }
      Plateau plateau = new Plateau(cases);
      XmlElement xEtat = xRoot["Etat"];
      if (xEtat == null)
      {
        throw new ApplicationException($"Fichier pilote incorrect, description état introuvable");
      }
      string sComplet = xEtat.GetAttribute("complet"), sReprise = xEtat.GetAttribute("reprise"), sSituationReprise = xEtat.GetAttribute("situation");
      if (!bool.TryParse(sComplet, out bool complet) || !Plateau.TryParseReprise(sReprise, out EnumReprise reprise))
      {
        throw new ApplicationException($"Fichier pilote incorrect, état non ou mal défini : {xEtat.OuterXml}");
      }
      byte[] situationReprise = null;
      if (reprise != EnumReprise.None && !Plateau.TryParseSituation(sSituationReprise, out situationReprise))
      {
        throw new ApplicationException($"Fichier pilote incorrect, état non ou mal défini : {xEtat.OuterXml}");
      }
      plateau.InitEtat(complet, reprise, situationReprise);
      foreach (XmlElement xSolution in xRoot.SelectNodes("Solution"))
      {
        string sSituation = xSolution.GetAttribute("situation");
        if (!TryParseSituation(sSituation, out byte[] situation))
        {
          throw new ApplicationException($"Fichier pilote incorrect, solution mal définie : {sSituation}");
        }
        List<int> mouvements = new List<int>();
        foreach (XmlElement xMvt in xSolution.SelectNodes("Mvt"))
        {
          string sidxMvt = xMvt.GetAttribute("idx");
          if (!int.TryParse(sidxMvt, out int idxMvt))
          {
            throw new ApplicationException($"Fichier pilote incorrect, mouvement de solution mal défini : {xMvt.OuterXml}");
          }
          mouvements.Add(idxMvt);
        }
        if (!plateau.AddSolution(situation, mouvements))
        {
          throw new ApplicationException($"Fichier pilote incorrect, solution mal définie : {xSolution.OuterXml}");
        }
      }
      return plateau;
    }

    internal void EnregistrerSolutions(DirectoryInfo repertoire)
    {
      string fichierPilote = Path.Combine(repertoire.FullName, NomFichierPilote);
      if (!File.Exists(fichierPilote))
      {
        throw new ApplicationException($"impossible de trouver le fichier pilote {fichierPilote}");
      }
      XmlDocument xDoc = new XmlDocument();
      // Peut déclencher une erreur si syntaxe incorrecte
      xDoc.Load(fichierPilote);
      XmlElement xRoot = xDoc.DocumentElement;
      // Suppression de la liste des solutions du fichier pilote
      foreach (XmlElement xSolution in xRoot.SelectNodes("Solution"))
      {
        xRoot.RemoveChild(xSolution);
      }
      // Création des noeuds solution 
      foreach (Solution solution in Solutions)
      {
        XmlElement xSolution = xDoc.CreateElement("Solution");
        xRoot.AppendChild(xSolution);
        xSolution.SetAttribute("situation", ToString(solution.Situation));
        foreach (int mvt in solution.Mouvements)
        {
          XmlElement xMvt = xDoc.CreateElement("Mvt");
          xSolution.AppendChild(xMvt);
          xMvt.SetAttribute("idx", mvt.ToString());
        }
      }
      xDoc.Save(fichierPilote);
    }

    private bool AddSolution(byte[] situation, List<int> mouvements)
    {
      // TODO : vérifier la conformité des données de la solution
      // vérifier que le flag de situation est correct : ses bits levés sont bien ceux de situations initiales
      // vérifier que les indices des mouvements sont valides
      // vérifier que ces mouvements sont possibles sur la situation passée en paramètre (non, car complexe)
      if (situation.Length != TailleDescriptionSituationEtMasque)
      {
        return false;
      }
      int nbPierres = NbPierres(situation);
      int nbMvt = mouvements.Count;
      if (nbMvt != 2 * nbPierres - NbCases)
      {
        return false;
      }

      Solution solution = new Solution(situation, mouvements);
      Solutions.Add(solution);
      return true;
    }

    private void InitEtat(bool complet, EnumReprise reprise, byte[] situationReprise)
    {
      // TODO : vérifier données fournies
      // vérifier que situationEtat est TailleDescriptionSituationEtMasque      
      // vérifier qu'elle décrit une situation existante avec les données stockées dans le niveau sous le plus élevé
      // vérifier que son flag est celui de cette situation existante
      Complet = complet;
      Reprise = reprise;
      if (reprise != EnumReprise.None)
      {
        if (situationReprise?.Length != TailleDescriptionSituationEtMasque)
        {
          throw new ArgumentException("situationReprise");
        }
        SituationReprise = new byte[TailleDescriptionSituationEtMasque];
        Array.Copy(situationReprise, SituationReprise, TailleDescriptionSituationEtMasque);
      }
      else
      {
        if (situationReprise != null)
        {
          throw new ArgumentException("situationReprise");
        }
        SituationReprise = null;
      }
    }

    public unsafe string ToString(byte* pElement)
    {
      StringBuilder sb = new StringBuilder();
      for (int idx = 0; idx < TailleDescriptionSituationEtMasque; idx++)
      {
        sb.Append($"{pElement[idx]:x}".PadLeft(2, '0')).Append(' ');
      }
      return sb.ToString();
    }

    public string ToString(byte[] pElement)
    {
      StringBuilder sb = new StringBuilder();
      for (int idx = 0; idx < TailleDescriptionSituationEtMasque; idx++)
      {
        sb.Append($"{pElement[idx]:x}".PadLeft(2, '0')).Append(' ');
      }
      return sb.ToString();
    }

    public string DescriptionPlateau()
    {
      char[,] caracteres = new char[TailleRectangleEnglobant.Width, TailleRectangleEnglobant.Height];
      for (int y = 0; y < TailleRectangleEnglobant.Height; y++)
      {
        for (int x = 0; x < TailleRectangleEnglobant.Width; x++)
        {
          caracteres[x, y] = ' ';
        }
      }
      foreach (var (x, y) in CoordonneesCases)
      {
        caracteres[x, y] = 'x';
      }
      StringBuilder sb = new StringBuilder();
      for (int y = 0; y < TailleRectangleEnglobant.Height; y++)
      {
        for (int x = 0; x < TailleRectangleEnglobant.Width; x++)
        {
          sb.Append(caracteres[x, y]);
        }
        sb.AppendLine();
      }
      return sb.ToString();
    }

    private static bool TryParseReprise(string sReprise, out EnumReprise reprise)
    {
      if (string.IsNullOrEmpty(sReprise))
      {
        reprise = EnumReprise.None;
        return true;
      }
      if (string.Compare(sReprise, "largeur", true) == 0)
      {
        reprise = EnumReprise.EnLargeur;
        return true;
      }
      if (string.Compare(sReprise, "profondeur", true) == 0)
      {
        reprise = EnumReprise.EnProfondeur;
        return true;
      }
      reprise = EnumReprise.None;
      return false;
    }
    private static bool TryParseSituation(string sSituation, out byte[] situation)
    {
      sSituation = Regex.Replace(sSituation, @"\s", "");
      situation = null;
      if (sSituation?.Length != 2 * TailleDescriptionSituationEtMasque)
      {
        return false;
      }
      situation = new byte[TailleDescriptionSituationEtMasque];
      for (int idx = 0; idx < TailleDescriptionSituationEtMasque; idx++)
      {
        string octet = sSituation.Substring(2 * idx, 2);
        if (!byte.TryParse(octet, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
        {
          return false;
        }
        situation[idx] = b;
      }
      return true;
    }

    // Le calcul du dual n'inclue pas dans le résultat le flag des situations initiales associées
    internal byte[] CalculeDual(byte[] situation)
    {
      for (int idxCase = 0; idxCase < NbCases; idxCase++)
      {
        if (PierrePresente(situation, idxCase))
        {
          EnlevePierre(situationDuale, idxCase);
        }
        else
        {
          PlacePierre(situationDuale, idxCase);
        }
      }
      return situationDuale;
    }

    internal bool GetSolutionPartielle(out Solution solution)
    {
      solution = null;
      foreach (Solution sol in Solutions)
      {
        int nbPierres = NbPierres(sol.Situation);
        if (nbPierres != NbCases - 1)
        {
          int nbMvt = sol.Mouvements.Count;
          if (nbMvt != 2 * nbPierres - NbCases)
          {
            throw new ApplicationException("Une solution partielle est inconsistante");
          }
          solution = sol;
          return true;
        }
      }
      return false;
    }
    internal void SupprimeSolutionPartielle(Solution solution)
    {
      Solutions.Remove(solution);
    }
    internal void AjouteSolutionsCompletes(List<Solution> solutionsCompletes)
    {
      Solutions.AddRange(solutionsCompletes);
    }
    internal void ConsoliderSolutionPartielle(Solution solution, byte[] situationFinale)
    {
      byte[] sit = new byte[TailleDescriptionSituationEtMasque];
      Array.Copy(solution.Situation, sit, TailleDescriptionSituationEtMasque);
      Debug.Print(Dump(sit));
      int iSym, iMvt, iMvtP;
      byte[] sitN = new byte[TailleDescriptionSituationEtMasque];
      byte[] sit1 = new byte[TailleDescriptionSituationEtMasque];
      byte[] sitP1 = new byte[TailleDescriptionSituationEtMasque];
      byte[] sitTmp;
      // Partie centrale, convertir les mouvements obtenus sur les situations normalisées
      // en les mouvements à effectuer sur la situation initiale de la solution partielle
      for (int iLoop = 0; iLoop < solution.Mouvements.Count; iLoop++)
      {
        Array.Copy(sit, sitN, TailleDescriptionSituationEtMasque);
        Normalise(sitN, out iSym);
        iMvt = solution.Mouvements[iLoop];
        Debug.Assert(MouvementAutorise(sitN, iMvt));
        MouvementEffectue(sitN, iMvt, sit1);
        if (iSym < 0)
        {
          iMvtP = iMvt;
        }
        else
        {
          iMvtP = MouvementsOriginels[iSym][iMvt];
        }
        Debug.Print($"sym {iSym}, mvt {DumpMvt(iMvtP)}");
        solution.Mouvements[iLoop] = iMvtP;
        Debug.Assert(MouvementAutorise(sit, iMvtP));
        MouvementEffectue(sit, iMvtP, sitP1);
        Debug.Print(Dump(sitP1));
        sitTmp = sit; sit = sitP1; sitP1 = sitTmp;
      }
      // A la sortie de la boucle précédente, sit[] contient la dernière situation obtenue et les solution.Mouvements ont été convertis.
      Array.Copy(sit, situationFinale, TailleDescriptionSituationEtMasque);
    }

    private string DumpMvt(int idxMvt0)
    {
      Mouvement mvt = MouvementsPossibles[idxMvt0];
      var (x1, y1) = CoordonneesCases[mvt.c1];
      var (x2, y2) = CoordonneesCases[mvt.c2];
      var (x3, y3) = CoordonneesCases[mvt.c3];
      return $"{{({x1},{y1});({x2},{y2});({x3},{y3})}}";
    }

    internal List<SolutionDetaillee> GetSolutionsDetaillees()
    {
      List<SolutionDetaillee> liste = new List<SolutionDetaillee>();
      foreach (Solution solution in Solutions)
      {
        if (solution.Mouvements.Count == NbCases - 2)
        {
          liste.Add(GetSolutionDetaillee(solution));
        }
      }
      return liste;
    }

    private SolutionDetaillee GetSolutionDetaillee(Solution solution)
    {
      SolutionDetaillee solutionDetaillee = new SolutionDetaillee(TailleRectangleEnglobant);
      for (int idxCase = 0; idxCase < NbCases; idxCase++)
      {
        var (x, y) = CoordonneesCases[idxCase];
        int valeurCase = PierrePresente(solution.Situation, idxCase) ? 1 : 0;
        solutionDetaillee.SituationInitiale[x, y] = valeurCase;
      }
      solutionDetaillee.Mouvements = new List<int[]>();
      foreach (int idxMvt in solution.Mouvements)
      {
        var mvt = MouvementsPossibles[idxMvt];        
        int[] m = new int[6];
        m[0] = CoordonneesCases[mvt.c1].x;
        m[1] = CoordonneesCases[mvt.c1].y;
        m[2] = CoordonneesCases[mvt.c2].x;
        m[3] = CoordonneesCases[mvt.c2].y;
        m[4] = CoordonneesCases[mvt.c3].x;
        m[5] = CoordonneesCases[mvt.c3].y;
        solutionDetaillee.Mouvements.Add(m);
      }

      return solutionDetaillee;
    }


    // Objectif : définir une situation sit aléatoire
    // Normaliser sit -> sitN
    // Appliquer un mouvement d'indice iMvt à sitN -> sit1
    // normaliser sit1 -> sit1N
    // Appliquer à sit le mouvement originel -> sit2
    // normaliser sit2 -> sit2N
    // comparer sit1N et sit2N : ils doivent être égaux.
    internal void TestMouvementsOriginaux()
    {
      int nbTests = 1000;
      Random random = new Random(0);
      for (int idxTest = 0; idxTest < nbTests; idxTest++)
      {
        byte[] sit = new byte[TailleDescriptionSituationEtMasque];
        // Placer des pierres de manière aléatoire
        // Il se peut qu'on place +sieurs fois une pierre dans une même case, 
        // ça permet de faire varier le nombre de pierres
        for (int idxCase = 0; idxCase < NbCases; idxCase++)
        {
          int idxCase2 = random.Next(NbCases - 1);
          PlacePierre(sit, idxCase2);
        }
        byte[] sitN = new byte[TailleDescriptionSituationEtMasque];
        Array.Copy(sit, sitN, sit.Length);
        Normalise(sitN, out int idxSymetrie);
        for (int idxMvt = 0; idxMvt < MouvementsPossibles.Count; idxMvt++)
        {
          if (MouvementAutorise(sitN, idxMvt))
          {
            byte[] sit1 = new byte[TailleDescriptionSituationEtMasque];
            MouvementEffectue(sitN, idxMvt, sit1);
            byte[] sit1N = new byte[TailleDescriptionSituationEtMasque];
            Array.Copy(sit1, sit1N, sit1.Length);
            Normalise(sit1N, out int idxSymetrie1);
            int idxMvtOri;
            if (idxSymetrie >= 0)
            {
              idxMvtOri = MouvementsOriginels[idxSymetrie][idxMvt];
            }
            else
            {
              idxMvtOri = idxMvt;
            }
            bool bMvtAutorise = MouvementAutorise(sit, idxMvtOri);
            if (!bMvtAutorise)
            {
              throw new ApplicationException("test échoué");
            }
            byte[] sit2 = new byte[TailleDescriptionSituationEtMasque];
            MouvementEffectue(sit, idxMvtOri, sit2);
            byte[] sit2N = new byte[TailleDescriptionSituationEtMasque];
            Array.Copy(sit2, sit2N, sit2.Length);
            Normalise(sit2N, out int idxSymetrie2);
            int nCmp = CompareSituations(sit1N, sit2N);
            if (nCmp != 0)
            {
              throw new ApplicationException("test échoué");
            }
          }
        }
      }
    }

  }

  internal enum EnumReprise
  {
    None,
    EnLargeur,
    EnProfondeur
  }
}
