using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class SituationsEtude : List<SituationEtude>
  {
    public SituationsEtude(Plateau plateau)
    {
      int count = plateau.NbCasesPlateau;
      for (int idx = 0; idx < count; idx++)
      {
        Add(new SituationEtude(plateau));
      }
    }
  }
}
