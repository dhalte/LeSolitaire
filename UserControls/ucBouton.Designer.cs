namespace UserControls
{
  partial class ucBouton
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
      this.pbBouton = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.pbBouton)).BeginInit();
      this.SuspendLayout();
      // 
      // pbBouton
      // 
      this.pbBouton.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pbBouton.Location = new System.Drawing.Point(0, 0);
      this.pbBouton.Name = "pbBouton";
      this.pbBouton.Size = new System.Drawing.Size(32, 32);
      this.pbBouton.TabIndex = 0;
      this.pbBouton.TabStop = false;
      this.pbBouton.Paint += new System.Windows.Forms.PaintEventHandler(this.pbBouton_Paint);
      this.pbBouton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbBouton_MouseDown);
      this.pbBouton.MouseEnter += new System.EventHandler(this.pbBouton_MouseEnter);
      this.pbBouton.MouseLeave += new System.EventHandler(this.pbBouton_MouseLeave);
      this.pbBouton.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbBouton_MouseMove);
      this.pbBouton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbBouton_MouseUp);
      // 
      // ucBouton
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.pbBouton);
      this.Name = "ucBouton";
      this.Size = new System.Drawing.Size(32, 32);
      ((System.ComponentModel.ISupportInitialize)(this.pbBouton)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox pbBouton;
  }
}
