using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class SituationDepart : Situation
  {
    public byte[] IdxSituationsInitiales;
    public SituationDepart(byte[] situationRaw, int nbSituationsInitiales) : base(situationRaw)
    {
      IdxSituationsInitiales = new byte[nbSituationsInitiales];
    }

    public SituationDepart(byte[] situationRaw, byte[] situationsNewAssociees) : base(situationRaw)
    {
      IdxSituationsInitiales = new byte[situationsNewAssociees.Length];
      Array.Copy(situationsNewAssociees, IdxSituationsInitiales, situationsNewAssociees.Length);
    }
  }
}
