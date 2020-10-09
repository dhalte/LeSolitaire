using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  class CompareSituations : IComparer<byte[]>
  {
    public int Compare(byte[] x, byte[] y)
    {
      int l = x.Length;
      for (int i = 0; i < l; i++)
      {
        if (x[i] != y[i]) return x[i].CompareTo(y[i]);
      }
      return 0;
    }
  }
}
