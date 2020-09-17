using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class SolutionMouvement
  {
    public SolutionMouvement(Coordonnee pierre, enumDirection mouvement)
    {
      Pierre = pierre;
      Direction = mouvement;
    }
    public readonly Coordonnee Pierre;
    public readonly enumDirection Direction;
  }
}
