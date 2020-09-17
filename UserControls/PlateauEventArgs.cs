using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserControls
{
  public class PlateauEventArgs:EventArgs
  {
    public PlateauEventArgs(PlateauInteraction plateauInteraction) { PlateauInteraction = plateauInteraction; }
    public PlateauInteraction PlateauInteraction;
  }
}
