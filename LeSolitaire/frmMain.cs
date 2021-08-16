using LeSolitaireLogique;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
//using LeSolitaireLogique;

namespace LeSolitaire
{
  public partial class frmMain : Form, Feedback
  {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private ActionControle ActionControle;

    public frmMain()
    {
      InitializeComponent();
    }

    private void frmMain_Load(object sender, EventArgs e)
    {
      ActionControle = new ActionControle(this);
      SwitchPanel(true);
    }

    private void mnuFichier_DropDownOpening(object sender, EventArgs e)
    {
      mnuInitialiser.Enabled = ActionControle.AutoriserInitialiser();
      mnuCharger.Enabled = ActionControle.AutoriserCharger();
    }

    private void mnuAction_DropDownOpening(object sender, EventArgs e)
    {
      mnuRechercheEnLargeur.Enabled = ActionControle.AutoriserRechercheEnLargeur();
      mnuRechercheEnProfondeur.Enabled = ActionControle.AutoriserRechercheEnProfondeur();
      mnuSuspendre.Enabled = ActionControle.AutoriserSuspendre();
      mnuConsolider.Enabled = ActionControle.AutoriserConsolider();
      mnuStats.Enabled = ActionControle.AutoriserStats();
      mnuVerifier.Enabled = ActionControle.AutoriserVerifier();
    }

    private void mnuVue_DropDownOpening(object sender, EventArgs e)
    {
      mnuResultats.Enabled = ActionControle.AutoriserVueResultats();
    }

    private void mnuInitialiser_Click(object sender, EventArgs e)
    {
      dlgChoixPlateau dlgChoixPlateau = new dlgChoixPlateau();
      if (dlgChoixPlateau.ShowDialog() != DialogResult.OK) return;
      dlgChoixRepertoire dlgChoixRepertoire = new dlgChoixRepertoire();
      if (dlgChoixRepertoire.ShowDialog() != DialogResult.OK) return;
      try
      {
        ActionControle.Initialiser(dlgChoixPlateau.Value, dlgChoixRepertoire.Value);
        ucSuivi.AddInfo("Jeux initialisé");
      }
      catch (ApplicationException ex)
      {
        SwitchPanel(true);
        ucSuivi.AddErreur(ex.Message);
      }
      catch (Exception ex)
      {
        SwitchPanel(true);
        ucSuivi.AddErreur(ex.ToString());
      }
    }

    private void mnuCharger_Click(object sender, EventArgs e)
    {
      dlgChoixRepertoire dlgChoixRepertoire = new dlgChoixRepertoire();
      if (dlgChoixRepertoire.ShowDialog() != DialogResult.OK) return;
      try
      {
        ActionControle.Charger(dlgChoixRepertoire.Value);
        ucSuivi.AddInfo("Jeu chargé");
      }
      catch (ApplicationException ex)
      {
        SwitchPanel(true);
        ucSuivi.AddErreur(ex.Message);
      }
      catch (Exception ex)
      {
        SwitchPanel(true);
        ucSuivi.AddErreur(ex.ToString());
      }
    }
    
    private delegate void delegateSwitchPanel(bool bShowSuivi);

    private void SwitchPanel(bool bShowSuivi)
    {
      if (this.InvokeRequired)
      {
        this.Invoke(new delegateSwitchPanel(SwitchPanel), bShowSuivi);
        return;
      }
      if (bShowSuivi)
      {
        ucSolutions.Hide();
        ucSolutions.Dock = DockStyle.None;
        ucSuivi.Dock = DockStyle.Fill;
        ucSuivi.Show();
        mnuResultats.Checked = false;
        mnuSuivi.Checked = true;
      }
      else
      {
        ucSuivi.Hide();
        ucSuivi.Dock = DockStyle.None;
        ucSolutions.Dock = DockStyle.Fill;
        ucSolutions.Init(ActionControle.Moteur);
        ucSolutions.Show();
        mnuSuivi.Checked = false;
        mnuResultats.Checked = true;
      }
    }
    public void Feedback(FeedbackHint hint, string msg)
    {
      switch (hint)
      {
        case FeedbackHint.trace:
        case FeedbackHint.warning:
          Logger.Warn(msg);
          ucSuivi.AddWarning(msg);
          break;
        case FeedbackHint.info:
          Logger.Info(msg);
          ucSuivi.AddInfo(msg);
          break;
        case FeedbackHint.startOfJob:
          Logger.Info($"Début du job {msg}");
          ucSuivi.AddInfo($"Début du job {msg}");
          break;
        case FeedbackHint.endOfJob:
          Logger.Info($"Fin du job {msg}");
          SwitchPanel(true);
          ucSuivi.AddInfo($"Fin du job {msg}");
          break;
        case FeedbackHint.error:
        default:
          Logger.Error(msg);
          SwitchPanel(true);
          ucSuivi.AddErreur(msg);
          break;
      }
    }

    private void mnuSuivi_Click(object sender, EventArgs e)
    {
      SwitchPanel(true);
    }

    private void mnuResultats_Click(object sender, EventArgs e)
    {
      SwitchPanel(false);
    }

    private void mnuRechercheEnLargeur_Click(object sender, EventArgs e)
    {
      ActionControle.LanceRechercheEnLargeur();
    }

    private void mnuRechercheEnProfondeur_Click(object sender, EventArgs e)
    {
      ActionControle.LanceRechercheEnProfondeur();
    }

    private void mnuConsolider_Click(object sender, EventArgs e)
    {
      ActionControle.LanceConsolidation();
    }

    private void mnuSuspendre_Click(object sender, EventArgs e)
    {
      ActionControle.Suspendre();
    }

    private void mnuStats_Click(object sender, EventArgs e)
    {
      ActionControle.LancerStats();
    }

    private void mnuVerifier_Click(object sender, EventArgs e)
    {
      ActionControle.LancerVerifier();
    }
  }
}
