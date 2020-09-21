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

    public int NbPierres => this.Count(c => c.pierre);
  }
}
