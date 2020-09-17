using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  // Utilisé pour décoder une description textuelle, 
  // Est assuré que les points sont triés par y croissant, et pour chaque y donné, par x croissant
  public class SituationRaw : List<(int x, int y, bool pierre)>
  {
    public Etendue Etendue;
    public SituationRaw()
    {
    }

    public SituationRaw(int capacity) : base(capacity)
    {
    }
    private void CalculeEtendue() => Etendue = Common.CalculeEtendueCentree(this); 
     public int NbPierres { get; private set; }
    public void CentrePoints()
    {
      CalculeEtendue();
      SituationRaw situationRaw = new SituationRaw(this.Count);
      this.ForEach(c => situationRaw.Add((c.x + Etendue.xMin, c.y + Etendue.yMin, c.pierre)));
      this.Clear();
      this.AddRange(situationRaw);
      NbPierres = this.Count(c => c.pierre);
    }
    
  }
}
