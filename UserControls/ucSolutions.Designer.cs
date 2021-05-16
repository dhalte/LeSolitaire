
namespace UserControls
{
  partial class ucSolutions
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
      this.cbListeSolutions = new System.Windows.Forms.ComboBox();
      this.ucAffichageSolution = new UserControls.ucPlateau();
      this.SuspendLayout();
      // 
      // cbListeSolutions
      // 
      this.cbListeSolutions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbListeSolutions.FormattingEnabled = true;
      this.cbListeSolutions.Location = new System.Drawing.Point(7, 2);
      this.cbListeSolutions.Margin = new System.Windows.Forms.Padding(6);
      this.cbListeSolutions.Name = "cbListeSolutions";
      this.cbListeSolutions.Size = new System.Drawing.Size(819, 21);
      this.cbListeSolutions.TabIndex = 2;
      // 
      // ucAffichageSolution
      // 
      this.ucAffichageSolution.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.ucAffichageSolution.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.ucAffichageSolution.Location = new System.Drawing.Point(5, 35);
      this.ucAffichageSolution.Margin = new System.Windows.Forms.Padding(6);
      this.ucAffichageSolution.Name = "ucAffichageSolution";
      this.ucAffichageSolution.Size = new System.Drawing.Size(823, 543);
      this.ucAffichageSolution.TabIndex = 3;
      // 
      // ucSolutions
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.ucAffichageSolution);
      this.Controls.Add(this.cbListeSolutions);
      this.Name = "ucSolutions";
      this.Size = new System.Drawing.Size(833, 584);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ComboBox cbListeSolutions;
    private ucPlateau ucAffichageSolution;
  }
}
