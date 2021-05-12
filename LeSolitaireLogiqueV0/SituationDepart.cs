using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogiqueV0
{
  public class SituationDepart : Situation
  {
    // On se limite à 8 situations initiales maximum (cas des plateaux classique et français)
    // et on gère un tableau de 8 bits.
    public byte IdxSituationsInitiales;
    public SituationDepart(byte[] situationRaw) : base(situationRaw)
    {
    }

    public SituationDepart(byte[] situationRaw, byte situationsNewAssociees) : base(situationRaw)
    {
      IdxSituationsInitiales = situationsNewAssociees;
    }
  }
}
