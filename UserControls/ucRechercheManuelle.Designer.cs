namespace UserControls
{
  partial class ucRechercheManuelle
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
      this.ucPlateau = new UserControls.ucPlateau();
      this.SuspendLayout();
      // 
      // ucPlateau
      // 
      this.ucPlateau.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.ucPlateau.Location = new System.Drawing.Point(0, 0);
      this.ucPlateau.Name = "ucPlateau";
      this.ucPlateau.Size = new System.Drawing.Size(1048, 487);
      this.ucPlateau.TabIndex = 0;
      // 
      // ucRechercheManuelle
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.ucPlateau);
      this.Name = "ucRechercheManuelle";
      this.Size = new System.Drawing.Size(1048, 527);
      this.ResumeLayout(false);

    }

    #endregion

    private ucPlateau ucPlateau;
  }
}
