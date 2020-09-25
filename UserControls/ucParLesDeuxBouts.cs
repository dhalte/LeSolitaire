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
using System.Diagnostics;
using System.Net.Configuration;
using System.Net.Http.Headers;

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
      Logique = new Logique(this);
      Logique.LanceRecherche(fileInfoSituationInitiale);
    }

    private void btSuspendre_Click(object sender, EventArgs e)
    {
      Feedback(enumFeedbackHint.info, "Demande postée");
      Logique?.StoppeBgTask();
    }

    private void SwitchDisplay(bool running)
    {
      ucFichier.Enabled = !running;
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

    private void btActions_Click(object sender, EventArgs e)
    {
      if (RegleActions())
      {
        ucFichier.Save();
        pnlActions.Show();
      }
    }

    private bool TaskInProgress()
    {
      if (Logique != null)
      {
        switch (Logique.State)
        {
          case enumState.running:
            RegleActions(enumOp.Suspendre);
            return true;
          case enumState.stopping:
            RegleActions(enumOp.None);
            return true;
          case enumState.stopped:
          default:
            Logique = null;
            break;
        }
      }
      return false;
    }

    private bool RegleActions()
    {
      if (TaskInProgress())
      {
        return true;
      }
      string fileName = ucFichier.Value.Trim();
      try
      {
        Logique = new Logique(this);
        enumOp enumOp = Logique.Verifie(fileName);
        RegleActions(enumOp);
        if ((enumOp & enumOp.ReglerNDNF) == enumOp.ReglerNDNF)
        {
          tbND.Text = Logique.Config.Pilote.Nd.ToString();
          tbNF.Text = Logique.Config.Pilote.Nf.ToString();
        }
      }
      catch (Exception ex)
      {
        Feedback(enumFeedbackHint.error, ex.Message);
        return false;
      }
      return true;
    }

    private void RegleActions(enumOp enumOp)
    {
      actionInitialiser.Enabled = enumOp == enumOp.Initialiser;
      actionConsoliderSolutions.Enabled = (enumOp & enumOp.ConsoliderSolutions) == enumOp.ConsoliderSolutions;
      actionRechercher.Enabled = (enumOp & enumOp.Rechercher) == enumOp.Rechercher;
      actionReglerND.Enabled = actionReglerNF.Enabled = (enumOp & enumOp.ReglerNDNF) == enumOp.ReglerNDNF;
      actionSuspendre.Enabled = enumOp == enumOp.Suspendre;
      tbND.Enabled = tbNF.Enabled = (enumOp & enumOp.ReglerNDNF) == enumOp.ReglerNDNF;
    }

    private void actionFermer_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      pnlActions.Hide();
    }

    private void actionEffacerSuivi_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      tbSuivi.Clear();
    }

    private void actionInitialiser_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      pnlActions.Hide();
      LanceInitialisation();
    }

    private void LanceInitialisation()
    {
      if (TaskInProgress())
      {
        pnlActions.Show();
        return;
      }
      string fileName = ucFichier.Value.Trim();
      try
      {
        Logique = new Logique(this);
        Logique.LanceInitialiser(fileName);
      }
      catch (Exception ex)
      {
        Feedback(enumFeedbackHint.error, ex.Message);
      }
    }

    private void actionReglerND_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      pnlActions.Hide();
      string sND = tbND.Text;
      bool bOK = int.TryParse(sND, out int nd);
      bOK = bOK && nd > 0;
      if (!bOK)
      {
        Feedback(enumFeedbackHint.error, "La valeur spécifiée est invalide");
        return;
      }
      // On appelle cette action après initialisation, donc Logique est déjà initialisé
      if (Logique.Config.Pilote.Nd == nd)
      {
        Feedback(enumFeedbackHint.error, "Modifiez la valeur pour calculer un nouveau fichier ED.dat");
        return;
      }
      Logique.LanceReglerND(nd);
    }

    private void actionReglerNF_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      pnlActions.Hide();
      string sNF = tbNF.Text;
      bool bOK = int.TryParse(sNF, out int nf);
      bOK = bOK && nf > 0;
      if (!bOK)
      {
        Feedback(enumFeedbackHint.error, "La valeur spécifiée est invalide");
        return;
      }
      // On appelle cette action après initialisation, donc Logique est déjà initialisé
      if (Logique.Config.Pilote.Nf == nf)
      {
        Feedback(enumFeedbackHint.error, "Modifiez la valeur pour calculer un nouveau fichier EF.dat");
        return;
      }
      Logique.LanceReglerNF(nf);
    }

    private void actionSuspendre_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      pnlActions.Hide();
      Feedback(enumFeedbackHint.info, "Demande postée");
      Logique?.StoppeBgTask();
    }
  }
}
