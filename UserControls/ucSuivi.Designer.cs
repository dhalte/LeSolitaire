
namespace UserControls
{
  partial class ucSuivi
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
      this.components = new System.ComponentModel.Container();
      this.tbSuivi = new System.Windows.Forms.RichTextBox();
      this.mnuContext = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.mnuClear = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuContext.SuspendLayout();
      this.SuspendLayout();
      // 
      // tbSuivi
      // 
      this.tbSuivi.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.tbSuivi.ContextMenuStrip = this.mnuContext;
      this.tbSuivi.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tbSuivi.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbSuivi.Location = new System.Drawing.Point(0, 0);
      this.tbSuivi.Name = "tbSuivi";
      this.tbSuivi.ReadOnly = true;
      this.tbSuivi.ShortcutsEnabled = false;
      this.tbSuivi.Size = new System.Drawing.Size(150, 150);
      this.tbSuivi.TabIndex = 0;
      this.tbSuivi.Text = "";
      // 
      // mnuContext
      // 
      this.mnuContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuClear});
      this.mnuContext.Name = "mnuContext";
      this.mnuContext.Size = new System.Drawing.Size(102, 26);
      // 
      // mnuClear
      // 
      this.mnuClear.Name = "mnuClear";
      this.mnuClear.Size = new System.Drawing.Size(101, 22);
      this.mnuClear.Text = "&Clear";
      this.mnuClear.Click += new System.EventHandler(this.mnuClear_Click);
      // 
      // ucSuivi
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tbSuivi);
      this.Name = "ucSuivi";
      this.mnuContext.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.RichTextBox tbSuivi;
    private System.Windows.Forms.ContextMenuStrip mnuContext;
    private System.Windows.Forms.ToolStripMenuItem mnuClear;
  }
}
