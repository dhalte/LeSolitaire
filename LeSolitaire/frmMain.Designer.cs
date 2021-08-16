using UserControls;

namespace LeSolitaire
{
  partial class frmMain
  {
    /// <summary>
    /// Variable nécessaire au concepteur.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Nettoyage des ressources utilisées.
    /// </summary>
    /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Code généré par le Concepteur Windows Form

    /// <summary>
    /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
    /// le contenu de cette méthode avec l'éditeur de code.
    /// </summary>
    private void InitializeComponent()
    {
      this.mainMenu = new System.Windows.Forms.MenuStrip();
      this.mnuFichier = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuInitialiser = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuCharger = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuAction = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRechercheEnLargeur = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRechercheEnProfondeur = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuSuspendre = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuConsolider = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuStats = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuVue = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuSuivi = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuResultats = new System.Windows.Forms.ToolStripMenuItem();
      this.ucSuivi = new UserControls.ucSuivi();
      this.ucSolutions = new UserControls.ucSolutions();
      this.mnuVerifier = new System.Windows.Forms.ToolStripMenuItem();
      this.mainMenu.SuspendLayout();
      this.SuspendLayout();
      // 
      // mainMenu
      // 
      this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFichier,
            this.mnuAction,
            this.mnuVue});
      this.mainMenu.Location = new System.Drawing.Point(0, 0);
      this.mainMenu.Name = "mainMenu";
      this.mainMenu.Size = new System.Drawing.Size(861, 24);
      this.mainMenu.TabIndex = 0;
      this.mainMenu.Text = "menuStrip1";
      // 
      // mnuFichier
      // 
      this.mnuFichier.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuInitialiser,
            this.mnuCharger});
      this.mnuFichier.Name = "mnuFichier";
      this.mnuFichier.Size = new System.Drawing.Size(54, 20);
      this.mnuFichier.Text = "&Fichier";
      this.mnuFichier.DropDownOpening += new System.EventHandler(this.mnuFichier_DropDownOpening);
      // 
      // mnuInitialiser
      // 
      this.mnuInitialiser.Name = "mnuInitialiser";
      this.mnuInitialiser.Size = new System.Drawing.Size(121, 22);
      this.mnuInitialiser.Text = "&Initialiser";
      this.mnuInitialiser.Click += new System.EventHandler(this.mnuInitialiser_Click);
      // 
      // mnuCharger
      // 
      this.mnuCharger.Name = "mnuCharger";
      this.mnuCharger.Size = new System.Drawing.Size(121, 22);
      this.mnuCharger.Text = "&Charger";
      this.mnuCharger.Click += new System.EventHandler(this.mnuCharger_Click);
      // 
      // mnuAction
      // 
      this.mnuAction.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuRechercheEnLargeur,
            this.mnuRechercheEnProfondeur,
            this.mnuSuspendre,
            this.mnuConsolider,
            this.mnuStats,
            this.mnuVerifier});
      this.mnuAction.Name = "mnuAction";
      this.mnuAction.Size = new System.Drawing.Size(54, 20);
      this.mnuAction.Text = "&Action";
      this.mnuAction.DropDownOpening += new System.EventHandler(this.mnuAction_DropDownOpening);
      // 
      // mnuRechercheEnLargeur
      // 
      this.mnuRechercheEnLargeur.Name = "mnuRechercheEnLargeur";
      this.mnuRechercheEnLargeur.Size = new System.Drawing.Size(208, 22);
      this.mnuRechercheEnLargeur.Text = "Recherche en &Largeur";
      this.mnuRechercheEnLargeur.Click += new System.EventHandler(this.mnuRechercheEnLargeur_Click);
      // 
      // mnuRechercheEnProfondeur
      // 
      this.mnuRechercheEnProfondeur.Name = "mnuRechercheEnProfondeur";
      this.mnuRechercheEnProfondeur.Size = new System.Drawing.Size(208, 22);
      this.mnuRechercheEnProfondeur.Text = "Recherche en &Profondeur";
      this.mnuRechercheEnProfondeur.Click += new System.EventHandler(this.mnuRechercheEnProfondeur_Click);
      // 
      // mnuSuspendre
      // 
      this.mnuSuspendre.Name = "mnuSuspendre";
      this.mnuSuspendre.Size = new System.Drawing.Size(208, 22);
      this.mnuSuspendre.Text = "&Suspendre";
      this.mnuSuspendre.Click += new System.EventHandler(this.mnuSuspendre_Click);
      // 
      // mnuConsolider
      // 
      this.mnuConsolider.Name = "mnuConsolider";
      this.mnuConsolider.Size = new System.Drawing.Size(208, 22);
      this.mnuConsolider.Text = "&Consolider";
      this.mnuConsolider.Click += new System.EventHandler(this.mnuConsolider_Click);
      // 
      // mnuStats
      // 
      this.mnuStats.Name = "mnuStats";
      this.mnuStats.Size = new System.Drawing.Size(208, 22);
      this.mnuStats.Text = "Stats";
      this.mnuStats.Click += new System.EventHandler(this.mnuStats_Click);
      // 
      // mnuVue
      // 
      this.mnuVue.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSuivi,
            this.mnuResultats});
      this.mnuVue.Name = "mnuVue";
      this.mnuVue.Size = new System.Drawing.Size(39, 20);
      this.mnuVue.Text = "&Vue";
      this.mnuVue.DropDownOpening += new System.EventHandler(this.mnuVue_DropDownOpening);
      // 
      // mnuSuivi
      // 
      this.mnuSuivi.Name = "mnuSuivi";
      this.mnuSuivi.Size = new System.Drawing.Size(121, 22);
      this.mnuSuivi.Text = "&Suivi";
      this.mnuSuivi.Click += new System.EventHandler(this.mnuSuivi_Click);
      // 
      // mnuResultats
      // 
      this.mnuResultats.Name = "mnuResultats";
      this.mnuResultats.Size = new System.Drawing.Size(121, 22);
      this.mnuResultats.Text = "&Résultats";
      this.mnuResultats.Click += new System.EventHandler(this.mnuResultats_Click);
      // 
      // ucSuivi
      // 
      this.ucSuivi.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.ucSuivi.Location = new System.Drawing.Point(319, 240);
      this.ucSuivi.Name = "ucSuivi";
      this.ucSuivi.Size = new System.Drawing.Size(542, 334);
      this.ucSuivi.TabIndex = 1;
      // 
      // ucSolutions
      // 
      this.ucSolutions.Location = new System.Drawing.Point(31, 93);
      this.ucSolutions.Name = "ucSolutions";
      this.ucSolutions.Size = new System.Drawing.Size(833, 584);
      this.ucSolutions.TabIndex = 2;
      this.ucSolutions.Visible = false;
      // 
      // mnuVerifier
      // 
      this.mnuVerifier.Name = "mnuVerifier";
      this.mnuVerifier.Size = new System.Drawing.Size(208, 22);
      this.mnuVerifier.Text = "&Vérifier";
      this.mnuVerifier.Click += new System.EventHandler(this.mnuVerifier_Click);
      // 
      // frmMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.ClientSize = new System.Drawing.Size(861, 574);
      this.Controls.Add(this.ucSolutions);
      this.Controls.Add(this.ucSuivi);
      this.Controls.Add(this.mainMenu);
      this.MainMenuStrip = this.mainMenu;
      this.Margin = new System.Windows.Forms.Padding(6);
      this.Name = "frmMain";
      this.Text = "Solitaire";
      this.Load += new System.EventHandler(this.frmMain_Load);
      this.mainMenu.ResumeLayout(false);
      this.mainMenu.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.MenuStrip mainMenu;
    private System.Windows.Forms.ToolStripMenuItem mnuFichier;
    private System.Windows.Forms.ToolStripMenuItem mnuInitialiser;
    private System.Windows.Forms.ToolStripMenuItem mnuCharger;
    private System.Windows.Forms.ToolStripMenuItem mnuAction;
    private System.Windows.Forms.ToolStripMenuItem mnuRechercheEnLargeur;
    private System.Windows.Forms.ToolStripMenuItem mnuRechercheEnProfondeur;
    private System.Windows.Forms.ToolStripMenuItem mnuSuspendre;
    private System.Windows.Forms.ToolStripMenuItem mnuVue;
    private System.Windows.Forms.ToolStripMenuItem mnuSuivi;
    private System.Windows.Forms.ToolStripMenuItem mnuResultats;
    private ucSuivi ucSuivi;
    private ucSolutions ucSolutions;
    private System.Windows.Forms.ToolStripMenuItem mnuConsolider;
    private System.Windows.Forms.ToolStripMenuItem mnuStats;
    private System.Windows.Forms.ToolStripMenuItem mnuVerifier;
  }
}

