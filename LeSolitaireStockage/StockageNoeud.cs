using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireStockage
{
  internal class StockageNoeud
  {
    internal bool dirty;
    internal byte[] data;
    // Ces deux membres restent à null pour les feuilles 
    internal UInt32[] OffsetEnfants;
    internal StockageNoeud[] Enfants;
    internal bool IsFeuille => Enfants == null;

    public StockageNoeud(int ordre, int tailleElement, bool feuille)
    {
      data = new byte[1 + (ordre - 1) * tailleElement];
      if (feuille)
      {
        Enfants = null;
        OffsetEnfants = null;
      }
      else
      {
        Enfants = new StockageNoeud[ordre];
        OffsetEnfants = new UInt32[ordre];
      }
    }

    public byte NbElements { get => data[0]; internal set => data[0] = value; }

    internal unsafe void Insert(byte* pSituationNew, byte idxInsertion, int tailleElement, StockageNoeud nouveauNoeud, UInt32 offsetNouveauNoeud)
    {
      if (NbElements > idxInsertion)
      {
        // Décalage des données de la taille d'un élément sur la droite à partir de idxInsertion
        Array.Copy(data, 1 + tailleElement * idxInsertion, data, 1 + tailleElement * (idxInsertion + 1), tailleElement * (NbElements - idxInsertion));
      }
      int borneInf = 1 + tailleElement * idxInsertion;
      // Copie des données du nouvel élément à l'index idxInsertion
      for (int idxInsert = 0; idxInsert < tailleElement; idxInsert++)
      {
        data[borneInf + idxInsert] = pSituationNew[idxInsert];
      }
      if (!IsFeuille)
      {
        if (NbElements > idxInsertion)
        {
          Array.Copy(Enfants, idxInsertion + 1, Enfants, idxInsertion + 2, NbElements - idxInsertion);
          Array.Copy(OffsetEnfants, idxInsertion + 1, OffsetEnfants, idxInsertion + 2, NbElements - idxInsertion);
        }
        OffsetEnfants[idxInsertion + 1] = offsetNouveauNoeud;
        Enfants[idxInsertion + 1] = nouveauNoeud;
      }
      NbElements++;
      dirty = true;
    }
  }
}
