using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTree
{
  internal class NoeudPersistant : NoeudVolatile
  {
    // Ce membre reste à null pour les feuilles 
    internal UInt32[] OffsetEnfants;

    public NoeudPersistant(int ordre, int tailleElement, bool feuille) : base(ordre, tailleElement, feuille)
    {
      if (feuille)
      {
        OffsetEnfants = null;
      }
      else
      {
        OffsetEnfants = new UInt32[ordre];
      }
    }
    internal override void InitEnfants(int ordre)
    {
      Enfants = new NoeudPersistant[ordre];
    }

    internal override unsafe void Insert(byte* pSituationNew, byte idxInsertion, int tailleElement, DataRemonteeVolatile dataRemontee)
    {
      base.Insert(pSituationNew, idxInsertion, tailleElement, dataRemontee);
      if (!IsFeuille)
      {
        if (NbElements > idxInsertion)
        {
          Array.Copy(OffsetEnfants, idxInsertion, OffsetEnfants, idxInsertion + 1, NbElements - idxInsertion);
        }
        OffsetEnfants[idxInsertion + 1] = ((DataRemonteePersistant)dataRemontee).offsetEnfantPlus;
      }
    }
    internal override void CopyEnfants(int idxFrom, NoeudVolatile nouveauNoeud, int idxTo, int nb)
    {
      base.CopyEnfants(idxFrom, nouveauNoeud, idxTo, nb);
      Array.Copy(OffsetEnfants, idxFrom, ((NoeudPersistant)nouveauNoeud).OffsetEnfants, idxTo, nb);
    }
    override internal void SetEnfant(int idxEnfant, DataRemonteeVolatile data)
    {
      base.SetEnfant(idxEnfant, data);
      OffsetEnfants[idxEnfant] = ((DataRemonteePersistant)data).offsetEnfantPlus;
    }
  }
}
