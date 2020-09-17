namespace UserControls
{
  partial class ucPlateau
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
      this.pbPlateau = new System.Windows.Forms.PictureBox();
      this.ucBoutons = new UserControls.ucBoutons();
      ((System.ComponentModel.ISupportInitialize)(this.pbPlateau)).BeginInit();
      this.SuspendLayout();
      // 
      // pbPlateau
      // 
      this.pbPlateau.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pbPlateau.Location = new System.Drawing.Point(0, 0);
      this.pbPlateau.Name = "pbPlateau";
      this.pbPlateau.Size = new System.Drawing.Size(216, 130);
      this.pbPlateau.TabIndex = 0;
      this.pbPlateau.TabStop = false;
      this.pbPlateau.Paint += new System.Windows.Forms.PaintEventHandler(this.pbPlateau_Paint);
      this.pbPlateau.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbPlateau_MouseDown);
      this.pbPlateau.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbPlateau_MouseMove);
      this.pbPlateau.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbPlateau_MouseUp);
      // 
      // ucBoutons
      // 
      this.ucBoutons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.ucBoutons.Location = new System.Drawing.Point(4, 137);
      this.ucBoutons.Name = "ucBoutons";
      this.ucBoutons.Size = new System.Drawing.Size(192, 32);
      this.ucBoutons.TabIndex = 1;
      this.ucBoutons.OnClic += new System.EventHandler<UserControls.ucBoutonEventArgs>(this.ucBoutons_OnClic);
      // 
      // ucPlateau
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.ucBoutons);
      this.Controls.Add(this.pbPlateau);
      this.Name = "ucPlateau";
      this.Size = new System.Drawing.Size(216, 171);
      this.Load += new System.EventHandler(this.ucPlateau_Load);
      this.Resize += new System.EventHandler(this.ucPlateau_Resize);
      ((System.ComponentModel.ISupportInitialize)(this.pbPlateau)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox pbPlateau;
    private ucBoutons ucBoutons;
  }
}
