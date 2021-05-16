using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  // Indique quelles opérations sont susceptibles de pouvoir être lancées
  [Flags]
  public enum enumOp
  {
    None=0,
    Initialiser=1,
    ConsoliderSolutions=2,
    Rechercher=4,
    ReglerNF=8,
    ArrangerND=16,
    Suspendre=32,
  }
}
