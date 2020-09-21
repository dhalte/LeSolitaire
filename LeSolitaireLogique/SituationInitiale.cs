using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class SituationInitiale
  {
    public bool Resolue;
    public Situation Situation;
    public SituationInitiale(Situation situation) 
    { 
      Situation = situation;
    }
  }
}
