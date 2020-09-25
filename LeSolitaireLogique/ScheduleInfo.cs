using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  class ScheduleInfo
  {
    // Petit utilitaire permettant d'émettre des infos régulières, de + en + espacées
    // Délivre un avis sur la pertinence du moment pour afficher cette information
    // Les  6 premières sont délivrées toutes les  5 secondes
    // les  6 suivantes sont délivrées toutes les 10 secondes
    // les  6 suivantes sont délivrées toutes les 30 secondes
    // les 10 suivantes sont délivrées toutes les 60 secondes
    // les 10 suivantes sont délivrées toutes les              5 minutes
    // les    suivantes sont délivrées toutes les             15 minutes
    private readonly (int sup, int tempo)[] tranches = { (6, 5), (6, 10), (6, 30), (10, 60), (10, 300), (int.MaxValue, 1500) };
    private int cptInfo;
    private int idxTranche;
    private DateTime nextDelivery;
    public void Start()
    {
      cptInfo = 0;
      idxTranche = 0;
      nextDelivery = DateTime.Now.AddSeconds(tranches[0].tempo);
    }
    public bool DelivreInfo()
    {
      if (DateTime.Now >= nextDelivery)
      {
        cptInfo++;
        if (idxTranche < tranches.Length - 1 && cptInfo > tranches[idxTranche].sup)
        {
          cptInfo = 0;
          idxTranche++;
        }
        nextDelivery = DateTime.Now.AddSeconds(tranches[idxTranche].tempo);
        return true;
      }
      return false;
    }
  }
}
