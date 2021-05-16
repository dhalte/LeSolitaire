using System;

namespace BTree
{
  // utilisé pour stocker les informations qui remontent au parent lors d'un split d'un de ses enfants.
  internal class DataRemonteeVolatile
  {
    internal byte[] elementRemonte;
    internal NoeudVolatile enfantPlus;

    internal DataRemonteeVolatile(int tailleElement)
    {
      elementRemonte = new byte[tailleElement];
    }
    internal virtual void ResetEnfant()
    {
      enfantPlus = null;
    }
  }
}
