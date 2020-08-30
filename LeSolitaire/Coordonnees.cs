using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaire
{
  // Stocke les objets Coordonnee immuables afin d'éviter de devoir les reconstruire systématiquement
  static class Coordonnees
  {
    static Coordonnee[,] StockCoordonnees;
    public static int xMin, xMax, yMin, yMax;
    public static void InitStock(int xMin, int xMax, int yMin, int yMax)
    {
      Coordonnees.xMin = xMin;
      Coordonnees.xMax = xMax;
      Coordonnees.yMin = yMin;
      Coordonnees.yMax = yMax;
      StockCoordonnees = new Coordonnee[xMax - xMin + 1, yMax - yMin + 1];
      for (int y = yMin; y <= yMax; y++)
      {
        for (int x = xMin; x <= xMax; x++)
        {
          StockCoordonnees[x - xMin, y - yMin] = new Coordonnee(x, y);
        }
      }
    }
    public static Coordonnee GetCoordonnee(int x, int y) => StockCoordonnees[x - xMin, y - yMin];

    internal static bool Contains(int x, int y) => xMin <= x && x <= xMax && yMin <= y && y <= yMax;

  }
}
