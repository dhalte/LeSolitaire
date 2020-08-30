using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaire
{
  public class Coordonnee
  {
    internal readonly int X;
    internal readonly int Y;
    public Coordonnee(int x, int y)
    {
      X = x;
      Y = y;
    }
    public override bool Equals(object obj)
    {
      Coordonnee c = obj as Coordonnee;
      if (c == null) return false;
      return c.X == X && c.Y == Y;
    }
    public override int GetHashCode()
    {
      int result = 7;
      unchecked
      {
        result = 31 * result + X;
        result = 31 * result + Y;
      }
      return result;
    }
    public override string ToString()
    {
      return $"({X},{Y})";
    }
    public Coordonnee Offset(int x, int y) => Coordonnees.GetCoordonnee(X + x, Y + y);

  }
}
