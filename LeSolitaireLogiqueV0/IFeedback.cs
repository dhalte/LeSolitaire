using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogiqueV0
{
  public interface IFeedback
  {
    void Feedback(enumFeedbackHint hint, string msg);
  }
}
