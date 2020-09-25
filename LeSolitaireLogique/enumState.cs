using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  // Gère l'état du thread asynchrone
  public enum enumState
  {
    running,
    stopping,
    stopped
  }
}
