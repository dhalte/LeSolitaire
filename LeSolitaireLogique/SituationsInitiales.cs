using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  // On a presque toujours besoin aussi bien 
  //  d'une liste des situations initiales pour y accéder par index
  //  que d'un hashset pour tester leur présence
  // Alors on couple les deux dans cette classe
  public class SituationsInitiales
  {
    private int nbPierres;
    public int NbPierres => nbPierres;
    public List<SituationInitiale> list = new List<SituationInitiale>();
    public HashSet<SituationBase> hashset = new HashSet<SituationBase>();
    public int Count => list.Count;
    public SituationInitiale this[int idx]
    {
      get => list[idx];
      set => list[idx] = value;
    }

    internal void Add(SituationInitiale situationInitiale)
    {
      if (list.Count==0 )
      {
        nbPierres = situationInitiale.NbPierres;
      }
      else if (nbPierres != situationInitiale.NbPierres)
      {
        throw new ApplicationException("Le nombre de pierres des situations initiales doit être le même pour toutes.");
      }
      list.Add(situationInitiale);
      hashset.Add(situationInitiale);
    }
  }
}
