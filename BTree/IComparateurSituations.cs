using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTree
{
  public interface IComparateurSituations
  {
    unsafe int CompareSituations(byte* p1, byte* p2);
    unsafe bool MajSituation(byte* pSituationNew, byte* pSituationExistante);
    unsafe string ToString(byte* pElement);
    string ToString(byte[] pElement);
  }
}
