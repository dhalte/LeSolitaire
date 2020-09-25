namespace UserControls
{
  partial class ucParLesDeuxBouts
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

    #region Code généré par le Concepteur de composants

    /// <summary> 
    /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
    /// le contenu de cette méthode avec l'éditeur de code.
    /// </summary>
    private void InitializeComponent()
    {
      this.tabMain = new System.Windows.Forms.TabControl();
      this.tabSuivi = new System.Windows.Forms.TabPage();
      this.tbSuivi = new System.Windows.Forms.TextBox();
      this.tabAffichageSolutions = new System.Windows.Forms.TabPage();
      this.ucAffichageSolution = new UserControls.ucPlateau();
      this.cbListeSolutions = new System.Windows.Forms.ComboBox();
      this.btActions = new System.Windows.Forms.Button();
      this.pnlActions = new System.Windows.Forms.Panel();
      this.actionEffacerSuivi = new System.Windows.Forms.LinkLabel();
      this.actionFermer = new System.Windows.Forms.LinkLabel();
      this.actionSuspendre = new System.Windows.Forms.LinkLabel();
      this.actionReglerNF = new System.Windows.Forms.LinkLabel();
      this.tbNF = new System.Windows.Forms.MaskedTextBox();
      this.actionReglerND = new System.Windows.Forms.LinkLabel();
      this.tbND = new System.Windows.Forms.MaskedTextBox();
      this.actionRechercher = new System.Windows.Forms.LinkLabel();
      this.actionConsoliderSolutions = new System.Windows.Forms.LinkLabel();
      this.actionInitialiser = new System.Windows.Forms.LinkLabel();
      this.ucFichier = new UserControls.ucListe();
      this.tabMain.SuspendLayout();
      this.tabSuivi.SuspendLayout();
      this.tabAffichageSolutions.SuspendLayout();
      this.pnlActions.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabMain
      // 
      this.tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tabMain.Controls.Add(this.tabSuivi);
      this.tabMain.Controls.Add(this.tabAffichageSolutions);
      this.tabMain.Location = new System.Drawing.Point(0, 25);
      this.tabMain.Name = "tabMain";
      this.tabMain.SelectedIndex = 0;
      this.tabMain.Size = new System.Drawing.Size(775, 378);
      this.tabMain.TabIndex = 3;
      // 
      // tabSuivi
      // 
      this.tabSuivi.Controls.Add(this.tbSuivi);
      this.tabSuivi.Location = new System.Drawing.Point(4, 22);
      this.tabSuivi.Name = "tabSuivi";
      this.tabSuivi.Padding = new System.Windows.Forms.Padding(3);
      this.tabSuivi.Size = new System.Drawing.Size(767, 352);
      this.tabSuivi.TabIndex = 0;
      this.tabSuivi.Text = "Suivi";
      this.tabSuivi.UseVisualStyleBackColor = true;
      // 
      // tbSuivi
      // 
      this.tbSuivi.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tbSuivi.Location = new System.Drawing.Point(3, 3);
      this.tbSuivi.Multiline = true;
      this.tbSuivi.Name = "tbSuivi";
      this.tbSuivi.ReadOnly = true;
      this.tbSuivi.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.tbSuivi.Size = new System.Drawing.Size(761, 346);
      this.tbSuivi.TabIndex = 0;
      // 
      // tabAffichageSolutions
      // 
      this.tabAffichageSolutions.Controls.Add(this.ucAffichageSolution);
      this.tabAffichageSolutions.Controls.Add(this.cbListeSolutions);
      this.tabAffichageSolutions.Location = new System.Drawing.Point(4, 22);
      this.tabAffichageSolutions.Name = "tabAffichageSolutions";
      this.tabAffichageSolutions.Padding = new System.Windows.Forms.Padding(3);
      this.tabAffichageSolutions.Size = new System.Drawing.Size(767, 352);
      this.tabAffichageSolutions.TabIndex = 1;
      this.tabAffichageSolutions.Text = "Solutions";
      this.tabAffichageSolutions.UseVisualStyleBackColor = true;
      // 
      // ucAffichageSolution
      // 
      this.ucAffichageSolution.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.ucAffichageSolution.Location = new System.Drawing.Point(0, 25);
      this.ucAffichageSolution.Name = "ucAffichageSolution";
      this.ucAffichageSolution.Size = new System.Drawing.Size(767, 324);
      this.ucAffichageSolution.TabIndex = 2;
      // 
      // cbListeSolutions
      // 
      this.cbListeSolutions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbListeSolutions.FormattingEnabled = true;
      this.cbListeSolutions.Location = new System.Drawing.Point(1, 0);
      this.cbListeSolutions.Name = "cbListeSolutions";
      this.cbListeSolutions.Size = new System.Drawing.Size(766, 21);
      this.cbListeSolutions.TabIndex = 1;
      // 
      // btActions
      // 
      this.btActions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btActions.Location = new System.Drawing.Point(696, 0);
      this.btActions.Name = "btActions";
      this.btActions.Size = new System.Drawing.Size(75, 23);
      this.btActions.TabIndex = 1;
      this.btActions.Text = "Actions";
      this.btActions.UseVisualStyleBackColor = true;
      this.btActions.Click += new System.EventHandler(this.btActions_Click);
      // 
      // pnlActions
      // 
      this.pnlActions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlActions.Controls.Add(this.actionEffacerSuivi);
      this.pnlActions.Controls.Add(this.actionFermer);
      this.pnlActions.Controls.Add(this.actionSuspendre);
      this.pnlActions.Controls.Add(this.actionReglerNF);
      this.pnlActions.Controls.Add(this.tbNF);
      this.pnlActions.Controls.Add(this.actionReglerND);
      this.pnlActions.Controls.Add(this.tbND);
      this.pnlActions.Controls.Add(this.actionRechercher);
      this.pnlActions.Controls.Add(this.actionConsoliderSolutions);
      this.pnlActions.Controls.Add(this.actionInitialiser);
      this.pnlActions.Location = new System.Drawing.Point(573, 0);
      this.pnlActions.Name = "pnlActions";
      this.pnlActions.Size = new System.Drawing.Size(198, 160);
      this.pnlActions.TabIndex = 2;
      this.pnlActions.Visible = false;
      // 
      // actionEffacerSuivi
      // 
      this.actionEffacerSuivi.AutoSize = true;
      this.actionEffacerSuivi.Location = new System.Drawing.Point(3, 138);
      this.actionEffacerSuivi.Name = "actionEffacerSuivi";
      this.actionEffacerSuivi.Size = new System.Drawing.Size(76, 13);
      this.actionEffacerSuivi.TabIndex = 9;
      this.actionEffacerSuivi.TabStop = true;
      this.actionEffacerSuivi.Text = "Effacer le suivi";
      this.actionEffacerSuivi.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.actionEffacerSuivi_LinkClicked);
      // 
      // actionFermer
      // 
      this.actionFermer.AutoSize = true;
      this.actionFermer.Location = new System.Drawing.Point(3, 119);
      this.actionFermer.Name = "actionFermer";
      this.actionFermer.Size = new System.Drawing.Size(79, 13);
      this.actionFermer.TabIndex = 8;
      this.actionFermer.TabStop = true;
      this.actionFermer.Text = "Fermer le menu";
      this.actionFermer.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.actionFermer_LinkClicked);
      // 
      // actionSuspendre
      // 
      this.actionSuspendre.AutoSize = true;
      this.actionSuspendre.Enabled = false;
      this.actionSuspendre.Location = new System.Drawing.Point(3, 81);
      this.actionSuspendre.Name = "actionSuspendre";
      this.actionSuspendre.Size = new System.Drawing.Size(58, 13);
      this.actionSuspendre.TabIndex = 6;
      this.actionSuspendre.TabStop = true;
      this.actionSuspendre.Text = "Suspendre";
      this.actionSuspendre.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.actionSuspendre_LinkClicked);
      // 
      // actionReglerNF
      // 
      this.actionReglerNF.AutoSize = true;
      this.actionReglerNF.Enabled = false;
      this.actionReglerNF.Location = new System.Drawing.Point(34, 43);
      this.actionReglerNF.Name = "actionReglerNF";
      this.actionReglerNF.Size = new System.Drawing.Size(55, 13);
      this.actionReglerNF.TabIndex = 4;
      this.actionReglerNF.TabStop = true;
      this.actionReglerNF.Text = "Régler NF";
      this.actionReglerNF.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.actionReglerNF_LinkClicked);
      // 
      // tbNF
      // 
      this.tbNF.Location = new System.Drawing.Point(7, 39);
      this.tbNF.Mask = "00";
      this.tbNF.Name = "tbNF";
      this.tbNF.PromptChar = ' ';
      this.tbNF.Size = new System.Drawing.Size(22, 20);
      this.tbNF.TabIndex = 3;
      this.tbNF.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // actionReglerND
      // 
      this.actionReglerND.AutoSize = true;
      this.actionReglerND.Enabled = false;
      this.actionReglerND.Location = new System.Drawing.Point(34, 24);
      this.actionReglerND.Name = "actionReglerND";
      this.actionReglerND.Size = new System.Drawing.Size(57, 13);
      this.actionReglerND.TabIndex = 2;
      this.actionReglerND.TabStop = true;
      this.actionReglerND.Text = "Régler ND";
      this.actionReglerND.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.actionReglerND_LinkClicked);
      // 
      // tbND
      // 
      this.tbND.AsciiOnly = true;
      this.tbND.CutCopyMaskFormat = System.Windows.Forms.MaskFormat.ExcludePromptAndLiterals;
      this.tbND.Location = new System.Drawing.Point(7, 20);
      this.tbND.Mask = "99";
      this.tbND.Name = "tbND";
      this.tbND.PromptChar = ' ';
      this.tbND.Size = new System.Drawing.Size(22, 20);
      this.tbND.TabIndex = 1;
      this.tbND.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.tbND.TextMaskFormat = System.Windows.Forms.MaskFormat.ExcludePromptAndLiterals;
      // 
      // actionRechercher
      // 
      this.actionRechercher.AutoSize = true;
      this.actionRechercher.Enabled = false;
      this.actionRechercher.Location = new System.Drawing.Point(3, 62);
      this.actionRechercher.Name = "actionRechercher";
      this.actionRechercher.Size = new System.Drawing.Size(63, 13);
      this.actionRechercher.TabIndex = 5;
      this.actionRechercher.TabStop = true;
      this.actionRechercher.Text = "Rechercher";
      // 
      // actionConsoliderSolutions
      // 
      this.actionConsoliderSolutions.AutoSize = true;
      this.actionConsoliderSolutions.Enabled = false;
      this.actionConsoliderSolutions.Location = new System.Drawing.Point(3, 100);
      this.actionConsoliderSolutions.Name = "actionConsoliderSolutions";
      this.actionConsoliderSolutions.Size = new System.Drawing.Size(116, 13);
      this.actionConsoliderSolutions.TabIndex = 7;
      this.actionConsoliderSolutions.TabStop = true;
      this.actionConsoliderSolutions.Text = "Consolider les solutions";
      // 
      // actionInitialiser
      // 
      this.actionInitialiser.AutoSize = true;
      this.actionInitialiser.Enabled = false;
      this.actionInitialiser.Location = new System.Drawing.Point(3, 5);
      this.actionInitialiser.Name = "actionInitialiser";
      this.actionInitialiser.Size = new System.Drawing.Size(47, 13);
      this.actionInitialiser.TabIndex = 0;
      this.actionInitialiser.TabStop = true;
      this.actionInitialiser.Text = "Initialiser";
      this.actionInitialiser.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.actionInitialiser_LinkClicked);
      // 
      // ucFichier
      // 
      this.ucFichier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.ucFichier.Location = new System.Drawing.Point(0, 0);
      this.ucFichier.Name = "ucFichier";
      this.ucFichier.Size = new System.Drawing.Size(690, 25);
      this.ucFichier.TabIndex = 0;
      // 
      // ucParLesDeuxBouts
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.pnlActions);
      this.Controls.Add(this.btActions);
      this.Controls.Add(this.tabMain);
      this.Controls.Add(this.ucFichier);
      this.Name = "ucParLesDeuxBouts";
      this.Size = new System.Drawing.Size(778, 406);
      this.Load += new System.EventHandler(this.ucParLesDeuxBouts_Load);
      this.tabMain.ResumeLayout(false);
      this.tabSuivi.ResumeLayout(false);
      this.tabSuivi.PerformLayout();
      this.tabAffichageSolutions.ResumeLayout(false);
      this.pnlActions.ResumeLayout(false);
      this.pnlActions.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion
    private System.Windows.Forms.TabControl tabMain;
    private System.Windows.Forms.TabPage tabSuivi;
    private System.Windows.Forms.TextBox tbSuivi;
    private System.Windows.Forms.TabPage tabAffichageSolutions;
    private ucListe ucFichier;
    private System.Windows.Forms.ComboBox cbListeSolutions;
    private ucPlateau ucAffichageSolution;
    private System.Windows.Forms.Button btActions;
    private System.Windows.Forms.Panel pnlActions;
    private System.Windows.Forms.LinkLabel actionInitialiser;
    private System.Windows.Forms.LinkLabel actionSuspendre;
    private System.Windows.Forms.LinkLabel actionReglerNF;
    private System.Windows.Forms.MaskedTextBox tbNF;
    private System.Windows.Forms.LinkLabel actionReglerND;
    private System.Windows.Forms.MaskedTextBox tbND;
    private System.Windows.Forms.LinkLabel actionRechercher;
    private System.Windows.Forms.LinkLabel actionConsoliderSolutions;
    private System.Windows.Forms.LinkLabel actionFermer;
    private System.Windows.Forms.LinkLabel actionEffacerSuivi;
  }
}
