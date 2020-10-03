using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class SolutionMouvement
  {
    public readonly byte IdxDepart;
    public readonly byte IdxSaut;

    public SolutionMouvement(byte idxPierre, byte idxSaut)
    {
      IdxDepart = idxPierre;
      IdxSaut = idxSaut;
    }    
    public byte IdxArrivee(Etendue etendue)
    {
      return etendue.IdxArrivee(IdxDepart, IdxSaut);
    }
  }
}
