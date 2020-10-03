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
      this.ucSolitaire = new UserControls.ucSolitaire();
      this.SuspendLayout();
      // 
      // ucSolitaire
      // 
      this.ucSolitaire.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.ucSolitaire.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ucSolitaire.Location = new System.Drawing.Point(0, 0);
      this.ucSolitaire.Margin = new System.Windows.Forms.Padding(11);
      this.ucSolitaire.Name = "ucSolitaire";
      this.ucSolitaire.Size = new System.Drawing.Size(861, 574);
      this.ucSolitaire.TabIndex = 5;
      // 
      // frmMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.ClientSize = new System.Drawing.Size(861, 574);
      this.Controls.Add(this.ucSolitaire);
      this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
      this.Name = "frmMain";
      this.Text = "Solitaire";
      this.Load += new System.EventHandler(this.Form1_Load);
      this.ResumeLayout(false);

    }

    #endregion

    private ucSolitaire ucSolitaire;
  }
}

