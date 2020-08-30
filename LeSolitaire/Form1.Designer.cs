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
      this.pbSolution = new System.Windows.Forms.PictureBox();
      this.pnlSolution = new System.Windows.Forms.Panel();
      this.btDown = new System.Windows.Forms.Button();
      this.btUp = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.pbSolution)).BeginInit();
      this.pnlSolution.SuspendLayout();
      this.SuspendLayout();
      // 
      // btRecherche
      // 
      this.btRecherche.Location = new System.Drawing.Point(12, 12);
      this.btRecherche.Name = "btRecherche";
      this.btRecherche.Size = new System.Drawing.Size(75, 23);
      this.btRecherche.TabIndex = 0;
      this.btRecherche.Text = "Rechercher";
      this.btRecherche.UseVisualStyleBackColor = true;
      this.btRecherche.Click += new System.EventHandler(this.btRecherche_Click);
      // 
      // pbSolution
      // 
      this.pbSolution.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pbSolution.Location = new System.Drawing.Point(0, 0);
      this.pbSolution.Name = "pbSolution";
      this.pbSolution.Size = new System.Drawing.Size(260, 231);
      this.pbSolution.TabIndex = 2;
      this.pbSolution.TabStop = false;
      this.pbSolution.Paint += new System.Windows.Forms.PaintEventHandler(this.pbSolution_Paint);
      // 
      // pnlSolution
      // 
      this.pnlSolution.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlSolution.Controls.Add(this.btDown);
      this.pnlSolution.Controls.Add(this.btUp);
      this.pnlSolution.Controls.Add(this.pbSolution);
      this.pnlSolution.Location = new System.Drawing.Point(12, 41);
      this.pnlSolution.Name = "pnlSolution";
      this.pnlSolution.Size = new System.Drawing.Size(260, 264);
      this.pnlSolution.TabIndex = 3;
      // 
      // btDown
      // 
      this.btDown.Location = new System.Drawing.Point(39, 238);
      this.btDown.Name = "btDown";
      this.btDown.Size = new System.Drawing.Size(29, 23);
      this.btDown.TabIndex = 3;
      this.btDown.Text = "->";
      this.btDown.UseVisualStyleBackColor = true;
      this.btDown.Click += new System.EventHandler(this.btDown_Click);
      // 
      // btUp
      // 
      this.btUp.Location = new System.Drawing.Point(4, 238);
      this.btUp.Name = "btUp";
      this.btUp.Size = new System.Drawing.Size(29, 23);
      this.btUp.TabIndex = 3;
      this.btUp.Text = "<-";
      this.btUp.UseVisualStyleBackColor = true;
      this.btUp.Click += new System.EventHandler(this.btUp_Click);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 317);
      this.Controls.Add(this.pnlSolution);
      this.Controls.Add(this.btRecherche);
      this.Name = "Form1";
      this.Text = "Solitaire";
      ((System.ComponentModel.ISupportInitialize)(this.pbSolution)).EndInit();
      this.pnlSolution.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btRecherche;
    private System.Windows.Forms.PictureBox pbSolution;
    private System.Windows.Forms.Panel pnlSolution;
    private System.Windows.Forms.Button btDown;
    private System.Windows.Forms.Button btUp;
  }
}

