using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  // Stocke les objets Coordonnee immuables afin d'éviter de devoir les reconstruire systématiquement
  public  class CoordonneesStock
  {
    public  Coordonnee[,] StockCoordonnees;
    public  Etendue Etendue;

    public CoordonneesStock(Etendue etendue)
    {
      Etendue = etendue;
      StockCoordonnees = new Coordonnee[Etendue.xMax - Etendue.xMin + 1, Etendue.yMax - Etendue.yMin + 1];
      for (int y = Etendue.yMin; y <= Etendue.yMax; y++)
      {
        for (int x = Etendue.xMin; x <= Etendue.xMax; x++)
        {
          StockCoordonnees[x - Etendue.xMin, y - Etendue.yMin] = new Coordonnee(x, y);
        }
      }
    }

    public  Coordonnee GetCoordonnee(int x, int y) => StockCoordonnees[x - Etendue.xMin, y - Etendue.yMin];

    public  Coordonnee GetCoordonnee(int idxCase) => StockCoordonnees[idxCase % (Etendue.xMax - Etendue.xMin + 1), idxCase / (Etendue.xMax - Etendue.xMin + 1)];

    public  bool Contains(int x, int y) => Etendue.Contains(x, y);
    public  int xMin { get => Etendue.xMin; }
    public  int xMax { get => Etendue.xMax; }
    public  int yMin { get => Etendue.yMin; }
    public  int yMax { get => Etendue.yMax; }
  }
}
