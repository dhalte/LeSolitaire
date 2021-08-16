using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique.Services
{
  public class SolutionDetaillee
  {
    public readonly (int Width, int Height) Dimensions;
    public int[,] SituationInitiale;

    internal SolutionDetaillee((int Width, int Height) dimensions)
    {
      Dimensions = dimensions;
      SituationInitiale = new int[dimensions.Width, dimensions.Height];
      for (int y = 0; y < dimensions.Height; y++)
      {
        for (int x = 0; x < dimensions.Width; x++)
        {
          SituationInitiale[x, y] = -1;
        }
      }
    }
    internal List<int[]> Mouvements;
    public int NbMouvements => Mouvements.Count;
    public int[,] CalculePlateau(int idxMvt)
    {
      int[,] resultat = new int[SituationInitiale.GetUpperBound(0) + 1, SituationInitiale.GetUpperBound(1) + 1];
      for (int y = 0; y < Dimensions.Height; y++)
      {
        for (int x = 0; x < Dimensions.Width; x++)
        {
          resultat[x, y] = SituationInitiale[x, y];
        }
      }
      for (int idxMvt1 = 0; idxMvt1 <= idxMvt; idxMvt1++)
      {
        var m = Mouvements[idxMvt1];
        resultat[m[0], m[1]] = 0;
        resultat[m[2], m[3]] = 0;
        resultat[m[4], m[5]] = 1;
      }
      return resultat;
    }
  }
}
