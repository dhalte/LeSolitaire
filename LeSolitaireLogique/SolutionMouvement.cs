using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class SolutionMouvement
  {
    public readonly byte IdxPierre;
    public readonly enumDirection Direction;

    public SolutionMouvement(byte idxPierre, enumDirection direction)
    {
      IdxPierre = idxPierre;
      Direction = direction;
    }
  }
}
