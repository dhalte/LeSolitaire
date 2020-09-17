using UserControls;

namespace LeSolitaire
{
  partial class Form1
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
      this.btRecherche = new System.Windows.Forms.Button();
      this.pnlSolution = new System.Windows.Forms.Panel();
      this.btRechercheMouvements = new System.Windows.Forms.Button();
      this.pbSolution = new System.Windows.Forms.PictureBox();
      this.btSuspendre = new System.Windows.Forms.Button();
      this.btDown = new System.Windows.Forms.Button();
      this.btUp = new System.Windows.Forms.Button();
      this.tabMain = new System.Windows.Forms.TabControl();
      this.tabChoixFichier = new System.Windows.Forms.TabPage();
      this.ucListeFichiers = new UserControls.ucListe();
      this.tabRechercheManuelle = new System.Windows.Forms.TabPage();
      this.ucPlateauManuel = new UserControls.ucPlateau();
      this.tabRechercheAutomatique = new System.Windows.Forms.TabPage();
      this.tabRechercheIncrementale = new System.Windows.Forms.TabPage();
      this.btRechercheIncrementale = new System.Windows.Forms.Button();
      this.tabParLesDeuxBouts = new System.Windows.Forms.TabPage();
      this.ucParLesDeuxBouts = new UserControls.ucParLesDeuxBouts();
      this.pnlSolution.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbSolution)).BeginInit();
      this.tabMain.SuspendLayout();
      this.tabChoixFichier.SuspendLayout();
      this.tabRechercheManuelle.SuspendLayout();
      this.tabRechercheAutomatique.SuspendLayout();
      this.tabRechercheIncrementale.SuspendLayout();
      this.tabParLesDeuxBouts.SuspendLayout();
      this.SuspendLayout();
      // 
      // btRecherche
      // 
      this.btRecherche.Location = new System.Drawing.Point(4, 3);
      this.btRecherche.Name = "btRecherche";
      this.btRecherche.Size = new System.Drawing.Size(75, 23);
      this.btRecherche.TabIndex = 0;
      this.btRecherche.Text = "Rechercher";
      this.btRecherche.UseVisualStyleBackColor = true;
      this.btRecherche.Click += new System.EventHandler(this.btRecherche_Click);
      // 
      // pnlSolution
      // 
      this.pnlSolution.Controls.Add(this.btRechercheMouvements);
      this.pnlSolution.Controls.Add(this.pbSolution);
      this.pnlSolution.Controls.Add(this.btSuspendre);
      this.pnlSolution.Controls.Add(this.btDown);
      this.pnlSolution.Controls.Add(this.btRecherche);
      this.pnlSolution.Controls.Add(this.btUp);
      this.pnlSolution.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlSolution.Location = new System.Drawing.Point(0, 0);
      this.pnlSolution.Name = "pnlSolution";
      this.pnlSolution.Size = new System.Drawing.Size(828, 422);
      this.pnlSolution.TabIndex = 3;
      // 
      // btRechercheMouvements
      // 
      this.btRechercheMouvements.Location = new System.Drawing.Point(185, 3);
      this.btRechercheMouvements.Name = "btRechercheMouvements";
      this.btRechercheMouvements.Size = new System.Drawing.Size(151, 23);
      this.btRechercheMouvements.TabIndex = 4;
      this.btRechercheMouvements.Text = "Recherche mouvements";
      this.btRechercheMouvements.UseVisualStyleBackColor = true;
      this.btRechercheMouvements.Click += new System.EventHandler(this.btRechercheMouvements_Click);
      // 
      // pbSolution
      // 
      this.pbSolution.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pbSolution.Location = new System.Drawing.Point(4, 32);
      this.pbSolution.Name = "pbSolution";
      this.pbSolution.Size = new System.Drawing.Size(821, 358);
      this.pbSolution.TabIndex = 2;
      this.pbSolution.TabStop = false;
      this.pbSolution.Paint += new System.Windows.Forms.PaintEventHandler(this.pbSolution_Paint);
      // 
      // btSuspendre
      // 
      this.btSuspendre.Enabled = false;
      this.btSuspendre.Location = new System.Drawing.Point(85, 3);
      this.btSuspendre.Name = "btSuspendre";
      this.btSuspendre.Size = new System.Drawing.Size(75, 23);
      this.btSuspendre.TabIndex = 3;
      this.btSuspendre.Text = "Suspendre";
      this.btSuspendre.UseVisualStyleBackColor = true;
      // 
      // btDown
      // 
      this.btDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btDown.Location = new System.Drawing.Point(39, 396);
      this.btDown.Name = "btDown";
      this.btDown.Size = new System.Drawing.Size(29, 23);
      this.btDown.TabIndex = 3;
      this.btDown.Text = "->";
      this.btDown.UseVisualStyleBackColor = true;
      this.btDown.Click += new System.EventHandler(this.btDown_Click);
      // 
      // btUp
      // 
      this.btUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btUp.Location = new System.Drawing.Point(4, 396);
      this.btUp.Name = "btUp";
      this.btUp.Size = new System.Drawing.Size(29, 23);
      this.btUp.TabIndex = 3;
      this.btUp.Text = "<-";
      this.btUp.UseVisualStyleBackColor = true;
      this.btUp.Click += new System.EventHandler(this.btUp_Click);
      // 
      // tabMain
      // 
      this.tabMain.Controls.Add(this.tabChoixFichier);
      this.tabMain.Controls.Add(this.tabRechercheManuelle);
      this.tabMain.Controls.Add(this.tabRechercheAutomatique);
      this.tabMain.Controls.Add(this.tabRechercheIncrementale);
      this.tabMain.Controls.Add(this.tabParLesDeuxBouts);
      this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabMain.Location = new System.Drawing.Point(0, 0);
      this.tabMain.Name = "tabMain";
      this.tabMain.SelectedIndex = 0;
      this.tabMain.Size = new System.Drawing.Size(836, 448);
      this.tabMain.TabIndex = 4;
      this.tabMain.SelectedIndexChanged += new System.EventHandler(this.tabMain_SelectedIndexChanged);
      // 
      // tabChoixFichier
      // 
      this.tabChoixFichier.Controls.Add(this.ucListeFichiers);
      this.tabChoixFichier.Location = new System.Drawing.Point(4, 22);
      this.tabChoixFichier.Name = "tabChoixFichier";
      this.tabChoixFichier.Padding = new System.Windows.Forms.Padding(3);
      this.tabChoixFichier.Size = new System.Drawing.Size(828, 422);
      this.tabChoixFichier.TabIndex = 0;
      this.tabChoixFichier.Text = "Choix fichier";
      this.tabChoixFichier.UseVisualStyleBackColor = true;
      // 
      // ucListeFichiers
      // 
      this.ucListeFichiers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.ucListeFichiers.Location = new System.Drawing.Point(9, 7);
      this.ucListeFichiers.Name = "ucListeFichiers";
      this.ucListeFichiers.Size = new System.Drawing.Size(811, 28);
      this.ucListeFichiers.TabIndex = 0;
      // 
      // tabRechercheManuelle
      // 
      this.tabRechercheManuelle.Controls.Add(this.ucPlateauManuel);
      this.tabRechercheManuelle.Location = new System.Drawing.Point(4, 22);
      this.tabRechercheManuelle.Name = "tabRechercheManuelle";
      this.tabRechercheManuelle.Padding = new System.Windows.Forms.Padding(3);
      this.tabRechercheManuelle.Size = new System.Drawing.Size(828, 422);
      this.tabRechercheManuelle.TabIndex = 1;
      this.tabRechercheManuelle.Text = "Recherche manuelle";
      this.tabRechercheManuelle.UseVisualStyleBackColor = true;
      // 
      // ucPlateauManuel
      // 
      this.ucPlateauManuel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ucPlateauManuel.Location = new System.Drawing.Point(3, 3);
      this.ucPlateauManuel.Name = "ucPlateauManuel";
      this.ucPlateauManuel.Size = new System.Drawing.Size(822, 416);
      this.ucPlateauManuel.TabIndex = 0;
      // 
      // tabRechercheAutomatique
      // 
      this.tabRechercheAutomatique.Controls.Add(this.pnlSolution);
      this.tabRechercheAutomatique.Location = new System.Drawing.Point(4, 22);
      this.tabRechercheAutomatique.Name = "tabRechercheAutomatique";
      this.tabRechercheAutomatique.Size = new System.Drawing.Size(828, 422);
      this.tabRechercheAutomatique.TabIndex = 2;
      this.tabRechercheAutomatique.Text = "Recherche automatique";
      this.tabRechercheAutomatique.UseVisualStyleBackColor = true;
      // 
      // tabRechercheIncrementale
      // 
      this.tabRechercheIncrementale.Controls.Add(this.btRechercheIncrementale);
      this.tabRechercheIncrementale.Location = new System.Drawing.Point(4, 22);
      this.tabRechercheIncrementale.Name = "tabRechercheIncrementale";
      this.tabRechercheIncrementale.Size = new System.Drawing.Size(828, 422);
      this.tabRechercheIncrementale.TabIndex = 3;
      this.tabRechercheIncrementale.Text = "Recherche incrémentale";
      this.tabRechercheIncrementale.UseVisualStyleBackColor = true;
      // 
      // btRechercheIncrementale
      // 
      this.btRechercheIncrementale.Location = new System.Drawing.Point(8, 12);
      this.btRechercheIncrementale.Name = "btRechercheIncrementale";
      this.btRechercheIncrementale.Size = new System.Drawing.Size(75, 23);
      this.btRechercheIncrementale.TabIndex = 0;
      this.btRechercheIncrementale.Text = "Recherche";
      this.btRechercheIncrementale.UseVisualStyleBackColor = true;
      this.btRechercheIncrementale.Click += new System.EventHandler(this.btRechercheIncrementale_Click);
      // 
      // tabParLesDeuxBouts
      // 
      this.tabParLesDeuxBouts.Controls.Add(this.ucParLesDeuxBouts);
      this.tabParLesDeuxBouts.Location = new System.Drawing.Point(4, 22);
      this.tabParLesDeuxBouts.Name = "tabParLesDeuxBouts";
      this.tabParLesDeuxBouts.Size = new System.Drawing.Size(828, 422);
      this.tabParLesDeuxBouts.TabIndex = 4;
      this.tabParLesDeuxBouts.Text = "Par les deux bouts";
      this.tabParLesDeuxBouts.UseVisualStyleBackColor = true;
      // 
      // ucParLesDeuxBouts
      // 
      this.ucParLesDeuxBouts.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ucParLesDeuxBouts.Location = new System.Drawing.Point(0, 0);
      this.ucParLesDeuxBouts.Name = "ucParLesDeuxBouts";
      this.ucParLesDeuxBouts.Size = new System.Drawing.Size(828, 422);
      this.ucParLesDeuxBouts.TabIndex = 0;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(836, 448);
      this.Controls.Add(this.tabMain);
      this.Name = "Form1";
      this.Text = "Solitaire";
      this.Load += new System.EventHandler(this.Form1_Load);
      this.pnlSolution.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pbSolution)).EndInit();
      this.tabMain.ResumeLayout(false);
      this.tabChoixFichier.ResumeLayout(false);
      this.tabRechercheManuelle.ResumeLayout(false);
      this.tabRechercheAutomatique.ResumeLayout(false);
      this.tabRechercheIncrementale.ResumeLayout(false);
      this.tabParLesDeuxBouts.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btRecherche;
    private System.Windows.Forms.PictureBox pbSolution;
    private System.Windows.Forms.Panel pnlSolution;
    private System.Windows.Forms.Button btDown;
    private System.Windows.Forms.Button btUp;
    private System.Windows.Forms.Button btSuspendre;
    private System.Windows.Forms.TabControl tabMain;
    private System.Windows.Forms.TabPage tabChoixFichier;
    private System.Windows.Forms.TabPage tabRechercheManuelle;
    private System.Windows.Forms.TabPage tabRechercheAutomatique;
    private ucListe ucListeFichiers;
    private ucPlateau ucPlateauManuel;
    private System.Windows.Forms.TabPage tabRechercheIncrementale;
    private System.Windows.Forms.Button btRechercheIncrementale;
    private System.Windows.Forms.Button btRechercheMouvements;
    private System.Windows.Forms.TabPage tabParLesDeuxBouts;
    private ucParLesDeuxBouts ucParLesDeuxBouts;
  }
}

