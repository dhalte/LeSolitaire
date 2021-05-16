using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTree
{
  internal class DataRemonteePersistant: DataRemonteeVolatile
  {
    internal UInt32 offsetEnfantPlus;
    internal DataRemonteePersistant(int tailleElement):base(tailleElement)
    {
    }
    internal override void ResetEnfant()
    {
      base.ResetEnfant();
      offsetEnfantPlus = UInt32.MaxValue;
    }
  }
}
