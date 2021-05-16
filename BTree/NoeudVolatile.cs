using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTree
{
  internal class NoeudVolatile
  {
    internal bool dirty;
    internal byte[] data;
    // Ce membre reste à null pour les feuilles 
    internal NoeudVolatile[] Enfants;
//    internal UInt32[] OffsetEnfants;
    internal NoeudVolatile(int ordre, int tailleElement, bool feuille)
    {
      data = new byte[1 + (ordre - 1) * tailleElement];
      if (feuille)
      {
        Enfants = null;
      }
      else
      {
        InitEnfants(ordre);
      }
    }
    internal virtual void InitEnfants(int ordre)
    {
      Enfants = new NoeudVolatile[ordre];
    }

    virtual internal unsafe void Insert(byte* pSituationNew, byte idxInsertion, int tailleElement, DataRemonteeVolatile dataRemontee)
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
        }
        Enfants[idxInsertion + 1] = dataRemontee.enfantPlus;
      }
      NbElements++;
      dirty = true;
    }
    internal bool IsFeuille => Enfants == null;
    internal byte NbElements { get => data[0]; set => data[0] = value; }

    virtual internal void CopyEnfants(int idxFrom, NoeudVolatile nouveauNoeud, int idxTo, int nb)
    {
      Array.Copy(Enfants, idxFrom, nouveauNoeud.Enfants, idxTo, nb);
    }
    virtual internal void SetEnfant(int idxEnfant, DataRemonteeVolatile data)
    {
      Enfants[idxEnfant] = data.enfantPlus;
    }
  }
}
