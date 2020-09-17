using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  class SituationsStock
  {
    private HashSet<SituationPacked>[] stock;
    // count est le nombre de pierres dans la situation initiale.
    // Nous allons gérer les stocks pour les situations allant de count-1 pierres à 1 pierre
    // donc nous attribuons un tableau de stocks rangés par nombre de pierres décroissantes.
    // [0] <=> count-1 pierres
    // ...
    // [count-2] <=> 1 pierre
    public SituationsStock(int count)
    {
      stock = new HashSet<SituationPacked>[count-1];
      for (int i = 0; i < stock.Length; i++)
      {
        stock[i] = new HashSet<SituationPacked>();
      }
    }

    public bool Contains(Situation situation)
    {
      HashSet<SituationPacked> stock = this.stock[this.stock.Length - situation.Count];
      // Debug.Print($"SituationsStock.Contains {Dump(situation.SituationCompacte)}");
      return stock.Contains(situation.SituationCompacte);
    }
    public void Add(Situation situation)
    {
      HashSet<SituationPacked> stock = this.stock[this.stock.Length - situation.Count];
      // Debug.Print($"SituationsStock.Add      {Dump(situation.SituationCompacte)}");
      stock.Add(situation.SituationCompacte);
    }
    private string Dump(SituationPacked situation)
    {
      return situation.Dump() + " " + situation.GetHashCode();
    }
    public override string ToString()
    {
      string sEntete = string.Empty;
      string sData = string.Empty;
      for (int i = 0; i < stock.Length; i++)
      {
        sEntete = sEntete + i.ToString() + " ";
        sData = sData + stock[i].Count() + " ";
      }
      return sEntete + Environment.NewLine + sData;
    }
  }
}
