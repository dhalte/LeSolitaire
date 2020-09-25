using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserControls
{
  public class ucBoutonEventArgs : EventArgs
  {
    public ucBoutonEventArgs(ucBoutonFx boutonFx) : base() { BoutonFx = boutonFx; }
    public readonly ucBoutonFx BoutonFx;
  }
}
