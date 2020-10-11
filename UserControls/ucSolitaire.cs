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
using System.Windows.Forms.VisualStyles;

namespace UserControls
{
  public partial class ucSolitaire : UserControl, IFeedback
  {
    public ucSolitaire()
    {
      InitializeComponent();
    }

    private Logique Logique;
    private SurveillanceMemoire Surveillance;

    private void ucParLesDeuxBouts_Load(object sender, EventArgs e)
    {
      ucFichier.Init("danielHalte/LeSolitaire/ParLesDeuxBouts");
      Surveillance = new SurveillanceMemoire();
      Surveillance.LowMemoryEvent += Surveillance_LowMemoryEvent;
      Surveillance.Start();
    }

    private void Surveillance_LowMemoryEvent(object sender, EventArgs e)
    {
      if (Logique != null) Logique.LowMemory = true;
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
      string head;
      switch (hint)
      {
        case enumFeedbackHint.trace:
          head = string.Empty;
          break;
        case enumFeedbackHint.info:
          head = $"{DateTime.Now:HH:mm:ss} {hint} ";
          break;
        case enumFeedbackHint.error:
          tabMain.SelectedTab = tabSuivi;
          head = $"{DateTime.Now:HH:mm:ss} {hint} ";
          break;
        case enumFeedbackHint.endOfJob:
          head = $"{DateTime.Now:HH:mm:ss} {hint} ";
          tabMain.SelectedTab = tabSuivi;
          SwitchDisplay(false);
          break;
        default:
          tabMain.SelectedTab = tabSuivi;
          head = $"{DateTime.Now:HH:mm:ss} {hint} ";
          break;
      }
      msg = $"{head}{msg}{Environment.NewLine}";
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
      actionReglerND.Enabled = (enumOp & enumOp.ReglerNDNF) == enumOp.ReglerNDNF;
      actionReglerNF.Enabled = (enumOp & enumOp.ReglerNDNF) == enumOp.ReglerNDNF;
      actionArrangerED.Enabled = (enumOp & enumOp.ArrangerND) == enumOp.ArrangerND;
      actionRechercher.Enabled = (enumOp & enumOp.Rechercher) == enumOp.Rechercher;
      actionSuspendre.Enabled = enumOp == enumOp.Suspendre;
      actionConsoliderSolutions.Enabled = (enumOp & enumOp.ConsoliderSolutions) == enumOp.ConsoliderSolutions;
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

    private void actionRechercher_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      pnlActions.Hide();
      Logique.LanceRecherche();
    }

    private void actionConsoliderSolutions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      pnlActions.Hide();
      Logique.LanceConsolider();
    }

    private void actionArrangerED_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      pnlActions.Hide();
      Logique.LanceArrangerED();
    }

    private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (tabMain.SelectedTab == tabAffichageSolutions)
      {

        try
        {
          if (!InitPanneauSolutions())
          {
            Feedback(enumFeedbackHint.error, "Spécifier une fiche de jeu valide");
            tabMain.SelectedTab = tabSuivi;
            return;
          }
        }
        catch (Exception ex)
        {
          Feedback(enumFeedbackHint.error, $"Spécifier une fiche de jeu valide : {ex.Message}");
        }
      }
    }

    private bool InitPanneauSolutions()
    {
      string filename = ucFichier.Value;
      if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
      {
        return false;
      }
      string directoryname = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));
      if (!Directory.Exists(directoryname))
      {
        return false;
      }
      string filenamepilote = Path.Combine(directoryname, Path.GetFileNameWithoutExtension(filename) + ".xml");
      if (!File.Exists(filenamepilote))
      {
        return false;
      }
      Pilote pilote = new Pilote(new FileInfo(filenamepilote));
      cbListeSolutions.SelectedIndexChanged -= cbListeSolutions_SelectedIndexChanged;
      cbListeSolutions.Items.Clear();
      for (int idxSolution = 0; idxSolution < pilote.Solutions.Count; idxSolution++)
      {
        // On inclut les solutions incomplètes
        string incomplete = pilote.Solutions[idxSolution].Complete ? "" : " (incomplete)";
        string item = $"solution {idxSolution + 1}{incomplete}";
        cbListeSolutions.Items.Add(item);
      }
      int idxSolutionChoisie = cbListeSolutions.Items.Count > 0 ? 0 : -1;
      cbListeSolutions.SelectedIndex = idxSolutionChoisie;
      cbListeSolutions.SelectedIndexChanged += cbListeSolutions_SelectedIndexChanged;      
      ucAffichageSolution.Init(pilote, idxSolutionChoisie);
      return true;
    }

    private void cbListeSolutions_SelectedIndexChanged(object sender, EventArgs e)
    {
      ucAffichageSolution.ChangeSolution(cbListeSolutions.SelectedIndex);
    }
  }
}
