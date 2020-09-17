using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class SituationPacked : List<byte>
  {

    public SituationPacked(int count) : base(count) { }
    public override bool Equals(object obj)
    {
      if (obj == this) return true;
      SituationPacked situationCompacte = obj as SituationPacked;
      if (situationCompacte == null) return false;
      int l = Count;
      if (l != situationCompacte.Count) return false;
      for (int i = 0; i < l; i++)
      {
        if (this[i] != situationCompacte[i])
        {
          return false;
        }
      }
      return true;
    }
    public override int GetHashCode()
    {
      int hashcode = 7;
      unchecked
      {
        for (int i = 0; i < this.Count; i++)
        {
          hashcode = 31 * hashcode + this[i];
        }
      }
      return hashcode;
    }

    internal string Dump()
    {
      string s = $"{this.Count:2} : ";
      this.ForEach(b => s = $"{s}{b:x2} ");
      return s;
    }
  }
}
