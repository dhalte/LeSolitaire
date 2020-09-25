using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class SituationInitiale : Situation
  {
    public bool Resolue;

    public SituationInitiale(Situation situation) : base(situation.Pierres)
    {
    }

  }
}
