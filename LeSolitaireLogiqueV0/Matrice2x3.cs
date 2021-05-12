using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogiqueV0
{
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
}
