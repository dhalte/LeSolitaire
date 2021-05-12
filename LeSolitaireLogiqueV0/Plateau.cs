using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace LeSolitaireLogiqueV0
{
  public class Plateau
  {
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

    public Plateau(SituationRaw situation)
    {
      Etendue = Common.CalculeEtendue(situation);
      PresenceCase = new bool[Etendue.Largeur * Etendue.Hauteur];
      Cases = new byte[situation.Count];
      int idxInCases = 0;
      foreach ((int x, int y, bool pierre) c in situation)
      {
        byte idxCase = Etendue.FromXY(c.x, c.y);
        PresenceCase[idxCase] = true;
        Cases[idxInCases++] = idxCase;
      }
      ConsoliderPlateau();
    }

    public bool Contains(int idxCase) => 0 <= idxCase && idxCase < Etendue.NbCasesRectangleEnglobant && PresenceCase[idxCase];

    public bool Contains(int x, int y) => 0 <= x && x < Etendue.Largeur && 0 <= y && y < Etendue.Hauteur && Contains(Etendue.FromXY(x, y));

    // Calculer les mouvements possibles dans le plateau
    // Calculer les symétries du plateau
    private void ConsoliderPlateau()
    {
      CalculMouvementsPossibles();
      CalculSymetries();
    }

    private void CalculMouvementsPossibles()
    {
      MouvementsPossibles = new List<(byte idxOrigine, byte idxVoisin, byte idxDestination)>();
      //                                                             Nord     Est     Sud     Ouest
      List<(int x, int y)> directions = new List<(int x, int y)>() { (0, -1), (1, 0), (0, 1), (-1, 0) };
      foreach (byte idxOrigine in Cases)
      {
        (int x, int y) origine = Etendue.FromByte(idxOrigine);
        foreach ((int x, int y) direction in directions)
        {
          int xVoisin = origine.x + direction.x, yVoisin = origine.y + direction.y,
              xDestination = xVoisin + direction.x, yDestination = yVoisin + direction.y;
          if (Etendue.Contains(xVoisin, yVoisin) && Etendue.Contains(xDestination, yDestination))
          {
            byte idxVoisin = Etendue.FromXY(xVoisin, yVoisin),
                idxDestination = Etendue.FromXY(xDestination, yDestination);
            // Les trois cases doivent être présentes dans le plateau. 
            // On ne parle pas des pierres ici.
            if (PresenceCase[idxVoisin] && PresenceCase[idxDestination])
            {
              MouvementsPossibles.Add((idxOrigine, idxVoisin, idxDestination));
            }
          }
        }
      }
    }

    private void CalculSymetries()
    {
      Symetries = new List<byte[]>();
      bool bCarre = Etendue.Largeur == Etendue.Hauteur;
      int xMin = 0, xMax = Etendue.Largeur - 1, yMin = 0, yMax = Etendue.Hauteur - 1;
      int nbCases = NbCasesPlateau;
      foreach (enumSymetries symetrie in Enum.GetValues(typeof(enumSymetries)))
      {
        Matrice2x3 M = null;
        switch (symetrie)
        {
          case enumSymetries.Id:
            M = new Matrice2x3((1, 0, 0), (0, 1, 0));
            break;
          case enumSymetries.rot90:
            if (bCarre)
            {
              M = new Matrice2x3((0, -1, xMin + yMax), (1, 0, -xMin + yMin));
            }
            break;
          case enumSymetries.rot180:
            M = new Matrice2x3((-1, 0, xMin + xMax), (0, -1, yMin + yMax));
            break;
          case enumSymetries.rot270:
            if (bCarre)
            {
              M = new Matrice2x3((0, 1, xMin - yMin), (-1, 0, xMin + yMax));
            }
            break;
          case enumSymetries.hor:
            M = new Matrice2x3((1, 0, 0), (0, -1, yMin + yMax));
            break;
          case enumSymetries.vert:
            M = new Matrice2x3((-1, 0, xMin + xMax), (0, 1, 0));
            break;
          case enumSymetries.premDiag:
            if (bCarre)
            {
              M = new Matrice2x3((0, 1, xMin - yMin), (1, 0, -xMin + yMin));
            }
            break;
          case enumSymetries.secondDiag:
            if (bCarre)
            {
              M = new Matrice2x3((0, -1, xMin + yMax), (-1, 0, xMin + yMax));
            }
            break;
        }
        if (M != null)
        {
          byte[] indirection = new byte[Etendue.NbCasesRectangleEnglobant];
          if (CalculeSymetries(M, indirection))
          {
            Symetries.Add(indirection);
          }
        }
      }
    }

    // Pour chaque indice de case du plateau, calcule l'indice de l'image par la symétrie donnée
    // Et si pour chaque case cette image est dans le plateau d'origine (si le plateau est globalement conservé)
    // alors enregistre les images de chaque case du plateau d'origine
    private bool CalculeSymetries(Matrice2x3 matrice, byte[] symetrie)
    {
      bool bOK = true;
      foreach (byte idxCasePlateau in Cases)
      {
        (int x, int y) coordonnees = Etendue.FromByte(idxCasePlateau);
        int x = matrice.m[0, 0] * coordonnees.x + matrice.m[0, 1] * coordonnees.y + matrice.m[0, 2];
        int y = matrice.m[1, 0] * coordonnees.x + matrice.m[1, 1] * coordonnees.y + matrice.m[1, 2];
        byte idxNewCase = Etendue.FromXY(x, y);
        if (PresenceCase[idxNewCase])
        {
          symetrie[idxCasePlateau] = idxNewCase;
        }
        else
        {
          bOK = false;
          break;
        }
      }
      return bOK;
    }

    public void GenereSymetrie(SituationEtude situationEtude, int idxSymetrie)
    {
      Array.Clear(situationEtude.ImagePierres, 0, situationEtude.ImagePierres.Length);
      byte[] symetrie = Symetries[idxSymetrie];
      for (int idxCase = 0; idxCase < Etendue.NbCasesRectangleEnglobant; idxCase++)
      {
        if (situationEtude.Pierres[idxCase])
        {
          int idxImage = symetrie[idxCase];
          situationEtude.ImagePierres[idxImage] = true;
        }
      }
    }
    public void GenereSymetrieMinimale(SituationEtude situationEtude, int idxSymetrie)
    {
      Array.Clear(situationEtude.ImagePierresMinimale, 0, situationEtude.ImagePierresMinimale.Length);
      byte[] symetrie = Symetries[idxSymetrie];
      for (int idxCase = 0; idxCase < Etendue.NbCasesRectangleEnglobant; idxCase++)
      {
        if (situationEtude.Pierres[idxCase])
        {
          int idxImage = symetrie[idxCase];
          situationEtude.ImagePierresMinimale[idxImage] = true;
        }
      }
    }

    // construction de la liste ordonnée des indices des cases du plateau
    // qui ne sont pas occupées par une pierre.
    // Sources : 
    //    byte[] Cases   : liste ordonnée des indices des cases du plateau
    //    byte[] pierres : liste ordonnée des indices des cases du plateau occupées par une pierre
    //    pierres est sous-ensemble de Cases
    // Résultat :
    //    byte[] dual    : liste ordonnée des indices des cases du plateau NON occupées par une pierre
    // Optimisation : on exploite le fait que les sources soient ordonnées pour éviter de rechercher
    //  pour chaque case du plateau si elle est présente ou non dans l'ensemble de pierres.
    internal byte[] SituationDualeRaw(byte[] pierres)
    {
      int idxInTblPierres = 0;
      byte[] dual = new byte[NbCasesPlateau - pierres.Length];
      int idxDual = 0;
      foreach (byte idxCasePlateau in Cases)
      {
        if (idxInTblPierres < pierres.Length)
        {
          if (idxCasePlateau < pierres[idxInTblPierres])
          {
            dual[idxDual++] = idxCasePlateau;
          }
          else
          {
            // on a nécessairement : 
            //   idxCasePlateau == pierres[idxInTblPierres]
            // et si idxInTblPierres+1 < pierres.Length
            //   idxCasePlateau < pierres[idxInTblPierres+1]
            idxInTblPierres++;
          }
        }
        else
        {
          dual[idxDual++] = idxCasePlateau;
        }
      }
      return dual;
    }
  }
}
