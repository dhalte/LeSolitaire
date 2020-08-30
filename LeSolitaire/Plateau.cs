using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaire
{
  class Plateau : HashSet<Coordonnee>
  {
    public void AddCase(Coordonnee coordonnee) => this.Add(coordonnee);
  }
}
