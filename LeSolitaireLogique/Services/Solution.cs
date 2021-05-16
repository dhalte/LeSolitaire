using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique.Services
{
  internal class Solution
  {
    internal byte[] Situation;
    internal List<int> Mouvements = new List<int>();

    public Solution(byte[] situation, List<int> mouvements)
    {
      Situation = new Byte[situation.Length];
      Array.Copy(situation, Situation, situation.Length);
      Mouvements.AddRange(mouvements);
    }
  }
}
