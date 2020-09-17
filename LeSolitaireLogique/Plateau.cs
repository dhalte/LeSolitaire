using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class Plateau : HashSet<Coordonnee>
  {
    public void AddCase(Coordonnee coordonnee) => this.Add(coordonnee);
    public Dictionary<Coordonnee, List<Coordonnee>> PointsSymetriques;
    public int NbSymetries;
    private enum enumSymetries
    {
      rot90,
      rot180,
      rot270,
      hor,
      vert,
      premDiag,
      secondDiag
    }
    class Matrice2x3
    {
      public int[,] m;
      public Matrice2x3((int x00, int x01, int x02) row0, (int x10, int x11, int x12) row1)
      {
        m = new int[2, 3];
        m[0, 0] = row0.x00;
        m[0, 1] = row0.x01;
        m[0, 2] = row0.x02;
        m[1, 0] = row1.x10;
        m[1, 1] = row1.x11;
        m[1, 2] = row1.x12;
      }
    }

    public void CalculeSymetries(CoordonneesStock coordonneesStock)
    {
      bool bCarre = coordonneesStock.xMax - coordonneesStock.xMin == coordonneesStock.yMax - coordonneesStock.yMin;
      PointsSymetriques = new Dictionary<Coordonnee, List<Coordonnee>>();
      NbSymetries = 0;
      Dictionary<Coordonnee, Coordonnee> result = new Dictionary<Coordonnee, Coordonnee>();

      foreach (Coordonnee item in this)
      {
        PointsSymetriques.Add(item, new List<Coordonnee>());
        result.Add(item, null);
      }

      foreach (enumSymetries symetrie in Enum.GetValues(typeof(enumSymetries)))
      {
        Matrice2x3 M = null;
        switch (symetrie)
        {
          case enumSymetries.rot90:
            if (bCarre)
            {
              M = new Matrice2x3((0, -1, coordonneesStock.xMin + coordonneesStock.yMax), (1, 0, -coordonneesStock.xMin + coordonneesStock.yMin));
            }
            break;
          case enumSymetries.rot180:
            M = new Matrice2x3((-1, 0, coordonneesStock.xMin + coordonneesStock.xMax), (0, -1, coordonneesStock.yMin + coordonneesStock.yMax));
            break;
          case enumSymetries.rot270:
            if (bCarre)
            {
              M = new Matrice2x3((0, 1, coordonneesStock.xMin - coordonneesStock.yMin), (-1, 0, coordonneesStock.xMin + coordonneesStock.yMax));
            }
            break;
          case enumSymetries.hor:
            M = new Matrice2x3((1, 0, 0), (0, -1, coordonneesStock.yMin + coordonneesStock.yMax));
            break;
          case enumSymetries.vert:
            M = new Matrice2x3((-1, 0, coordonneesStock.xMin + coordonneesStock.xMax), (0, 1, 0));
            break;
          case enumSymetries.premDiag:
            if (bCarre)
            {
              M = new Matrice2x3((0, 1, coordonneesStock.xMin - coordonneesStock.yMin), (1, 0, -coordonneesStock.xMin + coordonneesStock.yMin));
            }
            break;
          case enumSymetries.secondDiag:
            if (bCarre)
            {
              M = new Matrice2x3((0, -1, coordonneesStock.xMin + coordonneesStock.yMax), (-1, 0, coordonneesStock.xMin + coordonneesStock.yMax));
            }
            break;
        }
        if (M != null && CalculeSymetries(M, result, coordonneesStock))
        {
          AjouteSymetrie(result);
        }
      }

    }

    private bool CalculeSymetries(Matrice2x3 M, Dictionary<Coordonnee, Coordonnee> result, CoordonneesStock coordonneesStock)
    {
      bool bOK = true;
      foreach (Coordonnee item in this)
      {
        int x = M.m[0, 0] * item.X + M.m[0, 1] * item.Y + M.m[0, 2];
        int y = M.m[1, 0] * item.X + M.m[1, 1] * item.Y + M.m[1, 2];
        if (IsCaseDuPlateau(x, y, coordonneesStock))
        {
          result[item] = coordonneesStock.GetCoordonnee(x, y);
        }
        else
        {
          bOK = false;
          break;
        }
      }
      return bOK;
    }

    private bool IsCaseDuPlateau(int x, int y, CoordonneesStock coordonneesStock)
    {
      if (!coordonneesStock.Contains(x, y)) return false;
      return this.Contains(coordonneesStock.GetCoordonnee(x, y));
    }

    private void AjouteSymetrie(Dictionary<Coordonnee, Coordonnee> result)
    {
      foreach (KeyValuePair<Coordonnee, Coordonnee> kvp in result)
      {
        PointsSymetriques[kvp.Key].Add(kvp.Value);
      }
      ++NbSymetries;
    }

    internal IEnumerable<Situation> SituationsSymetriques(Situation situationNew)
    {
      for (int i = 0; i < NbSymetries; i++)
      {
        Situation situationSymetrique = new Situation();
        foreach (Coordonnee coordonnee in situationNew)
        {
          situationSymetrique.Add(PointsSymetriques[coordonnee][i]);
        }
        yield return situationSymetrique;
      }
    }

    public List<(Coordonnee A, Coordonnee B, Coordonnee C)> MouvementsPossibles;
    //Les coordonnées ne sont pas encore générées, et celles ci-dessous pourraient mêmes ne pas appartenir au stock
    private static readonly Coordonnee[] Directions = { new Coordonnee(1, 0), new Coordonnee(-1, 0), new Coordonnee(0, 1), new Coordonnee(0, -1) };
    public void CalculeMouvementsPossibles(CoordonneesStock coordonneesStock)
    {
      MouvementsPossibles = new List<(Coordonnee A, Coordonnee B, Coordonnee C)>();
      foreach (Coordonnee A in this)
      {
        foreach (Coordonnee offset in Directions)
        {
          int x, y;
          x = A.X + offset.X; y = A.Y + offset.Y;
          if (coordonneesStock.Contains(x, y))
          {
            Coordonnee B = coordonneesStock.GetCoordonnee(x, y);
            if (Contains(B))
            {
              x = B.X + offset.X; y = B.Y + offset.Y;
              if (coordonneesStock.Contains(x, y))
              {
                Coordonnee C = coordonneesStock.GetCoordonnee(x, y);
                if (Contains(C))
                {
                  MouvementsPossibles.Add((A, B, C));
                }
              }
            }
          }
        }
      }
    }
  }
}
