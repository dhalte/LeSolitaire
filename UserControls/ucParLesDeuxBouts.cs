using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using LeSolitaireLogique;

namespace UserControls
{
  public partial class ucParLesDeuxBouts : UserControl, IFeedback
  {
    public ucParLesDeuxBouts()
    {
      InitializeComponent();
    }

    private Logique Logique;
    private void ucParLesDeuxBouts_Load(object sender, EventArgs e)
    {
      ucFichier.Init("danielHalte/LeSolitaire/ParLesDeuxBouts");
      Logique = new Logique(this);
    }

    private void btRechercher_Click(object sender, EventArgs e)
    {
      tbSuivi.Clear();
      string fichierSituationInitiale = ucFichier.Value;
      FileInfo fileInfoSituationInitiale = null;
      try
      {
        fileInfoSituationInitiale = new FileInfo(fichierSituationInitiale);
        if (!fileInfoSituationInitiale.Exists)
        {
          throw new ApplicationException("Le fichier spécifié n'existe pas");
        }
      }
      catch (Exception ex)
      {
        Feedback(enumFeedbackHint.error, ex.Message);
        return;
      }
      ucFichier.Save();
      SwitchDisplay(true);
      Logique.LanceRecherche(fileInfoSituationInitiale);
    }

    private void btSuspendre_Click(object sender, EventArgs e)
    {
      Feedback(enumFeedbackHint.info, "Demande postée");
      Logique?.StoppeRecherche();
      btSuspendre.Visible = false;
    }

    private void SwitchDisplay(bool running)
    {
      ucFichier.Enabled = !running;
      btRechercher.Visible = !running;
      btSuspendre.Visible = running;
    }

    private delegate void FeedbackDelegate(enumFeedbackHint hint, string msg);
    public void Feedback(enumFeedbackHint hint, string msg)
    {
      if (InvokeRequired)
      {
        Invoke(new FeedbackDelegate(Feedback), hint, msg);
        return;
      }
      switch (hint)
      {
        case enumFeedbackHint.error:
          tabMain.SelectedTab = tabSuivi;
          break;
        case enumFeedbackHint.endOfJob:
          tabMain.SelectedTab = tabSuivi;
          SwitchDisplay(false);
          break;
        case enumFeedbackHint.trace:
        case enumFeedbackHint.info:
        default:
          break;
      }
      msg = $"{DateTime.Now:HH:mm:ss} {hint} {msg}{Environment.NewLine}";
      tbSuivi.AppendText(msg);
    }
  }
}
