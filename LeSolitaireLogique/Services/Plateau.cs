using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LeSolitaireStockage;

namespace LeSolitaireLogique.Services
{
  internal class Plateau : ComparateurSituations
  {
    // Les 5 octets décrivant l'état des cases d'une situation
    internal const int TailleDescriptionSituation = 5;
    // L'octet supplémentaire décrivant les situations initiales pouvant mener à la situation décrite
    internal const int TailleDescriptionSituationEtMasque = TailleDescriptionSituation + 1;
    private int NbCases;
    // Tableau permettant de connaitre l'indice d'une case du plateau en fonction de ses coordonnées
    // Les cases du plateau sont indicées à partir de 0.
    // Les cases du rectangle englobant qui ne sont pas du plateau ont un indice négatif.
    private (int Width, int Height) TailleRectangleEnglobant;
    private int[,] IndicesCasesInRectangleEnglobant;
    // Tableau permettant de connaitre les coordonnées d'une case sachant son indice
    private (int x, int y)[] CoordonneesCases;
    // Liste tous les mouvements possibles,
    // Chaque triplet (c1, c2, c3) représente les indices de 3 cases contigües alignées du plateau
    private List<(int c1, int c2, int c3)> MouvementsPossibles;

    // Indices et masques à utiliser pour manipuler les byte[] représentant l'état des cases d'un plateau
    // Ne dépend pas des coordonnées de ces cases
    private (int indice, byte masque, byte masqueComplementaire)[] IndicesEtMasquesCases;
    // Liste des symétries conservant globalement le plateau
    // On n'inclue pas l'identité dans ce tableau
    private List<int[]> SymetriesPlateau;
    // Ce buffer aura une taille de 5*(1+SymetriesPlateau.Count) octets.
    // Les 5 premiers octets contiennent une situation donnée.
    // Les groupes suivants de 5 octets contiennent les situations obtenues en appliquant une symétrie du plateau.
    private byte[] BufferSituation;

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
      foreach (var c in cases)
      {
        if (xMin > c.x) xMin = c.x;
        if (xMax < c.x) xMax = c.x;
        if (yMin > c.y) yMin = c.y;
        if (yMax < c.y) yMax = c.y;
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
        (int x, int y) c = cases[idx];
        CoordonneesCases[idx] = (c.x, c.y);
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
        (int x, int y) c = CoordonneesCases[idx];
        IndicesCasesInRectangleEnglobant[c.x, c.y] = idx;
      }
      #endregion Construction d'un tableau permettant de connaitre l'indice d'une case du plateau en fonction de ses coordonnées
      #region Recherche des mouvements possibles
      MouvementsPossibles = new List<(int c1, int c2, int c3)>();
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
            MouvementsPossibles.Add((IndicesCasesInRectangleEnglobant[xc1, yc1], IndicesCasesInRectangleEnglobant[xc2, yc2], IndicesCasesInRectangleEnglobant[xc3, yc3]));
          }
          // vers l'est
          xc2 = xc1 + 1; yc2 = yc1; xc3 = xc2 + 1; yc3 = yc2;
          if (!(xc2 > xMax || xc3 > xMax || IndicesCasesInRectangleEnglobant[xc2, yc2] < 0 || IndicesCasesInRectangleEnglobant[xc3, yc3] < 0))
          {
            MouvementsPossibles.Add((IndicesCasesInRectangleEnglobant[xc1, yc1], IndicesCasesInRectangleEnglobant[xc2, yc2], IndicesCasesInRectangleEnglobant[xc3, yc3]));
          }
          // vers le sud
          xc2 = xc1; yc2 = yc1 + 1; xc3 = xc2; yc3 = yc2 + 1;
          if (!(yc2 > yMax || yc3 > yMax || IndicesCasesInRectangleEnglobant[xc2, yc2] < 0 || IndicesCasesInRectangleEnglobant[xc3, yc3] < 0))
          {
            MouvementsPossibles.Add((IndicesCasesInRectangleEnglobant[xc1, yc1], IndicesCasesInRectangleEnglobant[xc2, yc2], IndicesCasesInRectangleEnglobant[xc3, yc3]));
          }
          // vers l'ouest
          xc2 = xc1 - 1; yc2 = yc1; xc3 = xc2 - 1; yc3 = yc2;
          if (!(xc2 < 0 || xc3 < 0 || IndicesCasesInRectangleEnglobant[xc2, yc2] < 0 || IndicesCasesInRectangleEnglobant[xc3, yc3] < 0))
          {
            MouvementsPossibles.Add((IndicesCasesInRectangleEnglobant[xc1, yc1], IndicesCasesInRectangleEnglobant[xc2, yc2], IndicesCasesInRectangleEnglobant[xc3, yc3]));
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
      #region Etablissement des symétries qui conservent globalement le plateau
      SymetriesPlateau = new List<int[]>();
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
          if (CalculeSymetriePlateau(matrice, indirection))
          {
            SymetriesPlateau.Add(indirection);
          }
        }
        BufferSituation = new byte[TailleDescriptionSituation * (1 + SymetriesPlateau.Count)];
      }
      #endregion Etablissement des symétries qui conservent globalement le plateau
    }

    private bool CalculeSymetriePlateau(Matrice2x3 matrice, int[] indirection)
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
          indirection[idxCase++] = IndicesCasesInRectangleEnglobant[x2, y2];
        }
      }
      return bOK;
    }

    internal unsafe bool CaseOccupee(byte* P, int i)
    {
      return (P[IndicesEtMasquesCases[i].indice] & IndicesEtMasquesCases[i].masque) != 0;
    }
    internal unsafe void PlacePierre(byte* P, int i)
    {
      P[IndicesEtMasquesCases[i].indice] |= IndicesEtMasquesCases[i].masque;
    }
    internal unsafe void EnlevePierre(byte* P, int i)
    {
      P[IndicesEtMasquesCases[i].indice] &= IndicesEtMasquesCases[i].masqueComplementaire;
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
    public unsafe bool MajSituation(byte* pSituationNew, byte* pSituationExistante)
    {
      byte* b1 = &pSituationExistante[TailleDescriptionSituationEtMasque - 1];
      byte b = (byte)(*b1 | pSituationNew[TailleDescriptionSituationEtMasque - 1]);
      if (*b1 == b)
      {
        return false;
      }
      *b1 = b;
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

    internal unsafe List<byte[]> CalculeSituationsInitiales()
    {
      List<byte[]> result = new List<byte[]>();
      fixed (byte* pBufferSituation = BufferSituation)
      {
        // Il faut gérer prudemment les derniers bits qui ne sont pas représentatifs d'une case, puisqu'il y a moins de cases que 5*8
        // On vide le plateau
        for (int idxByte = 0; idxByte < TailleDescriptionSituation; idxByte++)
        {
          *(pBufferSituation + idxByte) = 0;
        }
        // On le remplit de pierres
        for (int idxCase = 0; idxCase < NbCases; idxCase++)
        {
          PlacePierre(pBufferSituation, idxCase);
        }
        int idxPierrePrev = -1;
        // On va successivement enlever chaque pierre d'un plateau complet
        for (int idxPierre = 0; idxPierre < NbCases; idxPierre++)
        {
          // On remet la pierre précédemment enlevée
          if (idxPierrePrev >= 0)
          {
            PlacePierre(pBufferSituation, idxPierrePrev);
          }
          // On enlève exactement une pierre
          EnlevePierre(pBufferSituation, idxPierre);
          idxPierrePrev = idxPierre;
          // On génère toutes les images par symétrie de ce plateau initial, qu'on place dans les autres blocs de 5 octets de BufferSituation
          for (int idxSymetrie = 0; idxSymetrie < SymetriesPlateau.Count; idxSymetrie++)
          {
            int[] symetrie = SymetriesPlateau[idxSymetrie];
            int offset = (1 + idxSymetrie) * TailleDescriptionSituation;
            for (int idxCase = 0; idxCase < NbCases; idxCase++)
            {
              int idxCaseSym = symetrie[idxCase];
              if (CaseOccupee(pBufferSituation, idxCase))
              {
                PlacePierre(pBufferSituation + offset, idxCaseSym);
              }
              else
              {
                EnlevePierre(pBufferSituation + offset, idxCaseSym);
              }
            }
          }
#if DEBUG
          Debug.Print("--- liste des situations symétriques");
          for (int idxSymetrie = 0; idxSymetrie < 1 + SymetriesPlateau.Count; idxSymetrie++)
          {
            Debug.Print($"symétrie {idxSymetrie}");
            Debug.Print(Dump(pBufferSituation + idxSymetrie * TailleDescriptionSituation));
          }
#endif
          // Recherche de la situation de référence parmi toutes ces situations équivalentes
          int idxSituationRef = 0;
          for (int idxSymetrie = 1; idxSymetrie < 1 + SymetriesPlateau.Count; idxSymetrie++)
          {
            byte* situationRef = pBufferSituation + idxSituationRef * TailleDescriptionSituation;
            byte* situationCmp = pBufferSituation + idxSymetrie * TailleDescriptionSituation;
            if (CompareSituations(situationRef, situationCmp) > 0)
            {
              idxSituationRef = idxSymetrie;
            }
          }
          // Test si cette situation de référence est nouvelle dans la liste des situations initiales
          bool bExists = false;
          byte* situation = pBufferSituation + idxSituationRef * TailleDescriptionSituation;
          foreach (byte[] situationInitiale in result)
          {
            fixed (byte* pSituationInitiale = situationInitiale)
            {
              if (CompareSituations(pSituationInitiale, situation) == 0)
              {
                bExists = true;
                break;
              }
            }
          }
          if (!bExists)
          {
            // Ajout de la nouvelle situation de référence, avec son champ de flags l'identifiant
            byte[] nvlSituation = new byte[TailleDescriptionSituationEtMasque];
            for (int idxByte = 0; idxByte < TailleDescriptionSituation; idxByte++)
            {
              nvlSituation[idxByte] = *(situation + idxByte);
            }
            nvlSituation[TailleDescriptionSituationEtMasque - 1] = (byte)(1 << result.Count);
            result.Add(nvlSituation);
          }
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
        (int x, int y) c = CoordonneesCases[idxCase];
        if (CaseOccupee(situation, idxCase))
        {
          result[c.x, c.y] = 'x';
        }
        else
        {
          result[c.x, c.y] = 'o';
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

    private const string NomFichierPilote = "pilote.xml";

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
        (int x, int y) c = CoordonneesCases[idxcase];
        XmlElement xCase = xDoc.CreateElement("Case");
        plateau.AppendChild(xCase);
        xCase.SetAttribute("x", c.x.ToString());
        xCase.SetAttribute("y", c.y.ToString());
      }
      XmlElement etat = xDoc.CreateElement("Etat");
      root.AppendChild(etat);
      etat.SetAttribute("complet", "false");
      etat.SetAttribute("reprise", "");
      etat.SetAttribute("situation", "");
      xDoc.Save(Path.Combine(repertoire.FullName, NomFichierPilote));
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
  }
}
