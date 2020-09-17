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
      this.btSuspendre = new System.Windows.Forms.Button();
      this.tabMain = new System.Windows.Forms.TabControl();
      this.tabSuivi = new System.Windows.Forms.TabPage();
      this.tbSuivi = new System.Windows.Forms.TextBox();
      this.tabAffichageSolutions = new System.Windows.Forms.TabPage();
      this.cbListeSolutions = new System.Windows.Forms.ComboBox();
      this.ucFichier = new UserControls.ucListe();
      this.btRechercher = new System.Windows.Forms.Button();
      this.ucAffichageSolution = new UserControls.ucPlateau();
      this.tabMain.SuspendLayout();
      this.tabSuivi.SuspendLayout();
      this.tabAffichageSolutions.SuspendLayout();
      this.SuspendLayout();
      // 
      // btSuspendre
      // 
      this.btSuspendre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btSuspendre.Location = new System.Drawing.Point(681, 0);
      this.btSuspendre.Name = "btSuspendre";
      this.btSuspendre.Size = new System.Drawing.Size(97, 29);
      this.btSuspendre.TabIndex = 7;
      this.btSuspendre.Text = "Suspendre";
      this.btSuspendre.UseVisualStyleBackColor = true;
      this.btSuspendre.Visible = false;
      this.btSuspendre.Click += new System.EventHandler(this.btSuspendre_Click);
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
      this.tabMain.TabIndex = 6;
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
      // ucFichier
      // 
      this.ucFichier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.ucFichier.Location = new System.Drawing.Point(0, 0);
      this.ucFichier.Name = "ucFichier";
      this.ucFichier.Size = new System.Drawing.Size(675, 25);
      this.ucFichier.TabIndex = 5;
      // 
      // btRechercher
      // 
      this.btRechercher.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btRechercher.Location = new System.Drawing.Point(681, 0);
      this.btRechercher.Name = "btRechercher";
      this.btRechercher.Size = new System.Drawing.Size(97, 29);
      this.btRechercher.TabIndex = 8;
      this.btRechercher.Text = "Rechercher";
      this.btRechercher.UseVisualStyleBackColor = true;
      this.btRechercher.Click += new System.EventHandler(this.btRechercher_Click);
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
      // ucParLesDeuxBouts
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.btRechercher);
      this.Controls.Add(this.btSuspendre);
      this.Controls.Add(this.tabMain);
      this.Controls.Add(this.ucFichier);
      this.Name = "ucParLesDeuxBouts";
      this.Size = new System.Drawing.Size(778, 406);
      this.Load += new System.EventHandler(this.ucParLesDeuxBouts_Load);
      this.tabMain.ResumeLayout(false);
      this.tabSuivi.ResumeLayout(false);
      this.tabSuivi.PerformLayout();
      this.tabAffichageSolutions.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btSuspendre;
    private System.Windows.Forms.TabControl tabMain;
    private System.Windows.Forms.TabPage tabSuivi;
    private System.Windows.Forms.TextBox tbSuivi;
    private System.Windows.Forms.TabPage tabAffichageSolutions;
    private ucListe ucFichier;
    private System.Windows.Forms.Button btRechercher;
    private System.Windows.Forms.ComboBox cbListeSolutions;
    private ucPlateau ucAffichageSolution;
  }
}
