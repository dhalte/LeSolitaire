using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class SituationStock
  {
    private HashSet<SituationBase >[] stocks;
    public SituationStock(int count)
    {
      stocks = new HashSet<SituationBase>[count];
      for (int idx = 0; idx < count; idx++)
      {
        stocks[idx] = new HashSet<SituationBase>();
      }
    }
    public bool Contains(SituationEtude situationEtude)
    {
      int nbPierres = situationEtude.NbPierres;
      return stocks[nbPierres].Contains(situationEtude);
    }
    public void Add(Situation situation)
    {
      int nbPierres = situation.NbPierres;
      bool b = stocks[nbPierres].Add(situation);
      if (!b) Debugger.Break();
    }
  }
}
