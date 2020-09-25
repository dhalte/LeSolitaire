using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  // Liste issue du décodage d'une description textuelle d'une situation
  // Chaque terme est une présence, soit d'une pierre, soit d'une case vide pouvant accueillir une pierre
  // Les coordonnées minimales sont assurées être 0 par l'interpréteur de description
  public class SituationRaw : List<(int x, int y, bool pierre)>
  {
    public bool ControleDescription(string descriptionSituation)
    {
      SituationRaw situationRaw = Common.ChargeSituationRaw(descriptionSituation);
      if (Count != situationRaw.Count)
      {
        return false;
      }
      for (int idx = 0; idx < Count; idx++)
      {
        (int x, int y, bool pierre) casePlateau = this[idx];
        (int x, int y, bool pierre) caseSituation = situationRaw[idx];
        if (casePlateau.x != caseSituation.x || casePlateau.y != caseSituation.y || casePlateau.pierre != caseSituation.pierre)
        {
          return false;
        }
      }
      return true;
    }
    public int NbPierres => this.Count(c => c.pierre);
  }
}
