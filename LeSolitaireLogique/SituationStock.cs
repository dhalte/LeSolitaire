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
    // Un tableau [] de Hashsets
    private HashSet<SituationBase>[] stocks;
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

    internal void LibereMemoire(int idxStart, int idxStop)
    {
      long[] tailleMemoireParStock = new long[idxStop - idxStart];
      long tailleMemoireGlobale = 0;
      for (int idxStock = idxStart; idxStock < idxStop; idxStock++)
      {
        HashSet<SituationBase> stock = stocks[idxStock];
        // Les objets dans le stock sont essentiellement des tableaux d'octets
        // dont la taille est fonction de l'indice de la case dans laquelle ils sont placés.
        int tailleStock = stock.Count * idxStock;
        tailleMemoireParStock[idxStock - idxStart] = tailleStock;
        tailleMemoireGlobale += tailleStock;
        Debug.Print($"idxStock={idxStock}, taille={tailleStock}");
      }
      // On va libérer au moins 50% de la mémoire occupée
      tailleMemoireGlobale /= 2;
      // On va libérer de préférence les stocks proches de SF, c'est à dire proches de idxStart
      for (int idxStock = idxStart; idxStock < idxStop && tailleMemoireGlobale > 0; idxStock++)
      {
        stocks[idxStock].Clear();
        tailleMemoireGlobale -= tailleMemoireParStock[idxStock - idxStart];
        Debug.Print($"libération de idxStock={idxStock}");
      }
    }
  }
}
