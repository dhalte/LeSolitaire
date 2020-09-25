using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public interface IFeedback
  {
    void Feedback(enumFeedbackHint hint, string msg);
  }
}
