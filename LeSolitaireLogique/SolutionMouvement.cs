using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class SolutionMouvement
  {
    public readonly byte IdxDdepart;
    public readonly byte IdxSaut;

    public SolutionMouvement(byte idxPierre, byte idxSaut)
    {
      IdxDdepart = idxPierre;
      IdxSaut = idxSaut;
    }
  }
}
