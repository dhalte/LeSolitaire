namespace UserControls
{
  partial class ucListe
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
      this.cbListe = new System.Windows.Forms.ComboBox();
      this.btOuvrir = new System.Windows.Forms.Button();
      this.btSupprimer = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // cbListe
      // 
      this.cbListe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbListe.FormattingEnabled = true;
      this.cbListe.Location = new System.Drawing.Point(2, 0);
      this.cbListe.Margin = new System.Windows.Forms.Padding(0);
      this.cbListe.Name = "cbListe";
      this.cbListe.Size = new System.Drawing.Size(446, 21);
      this.cbListe.TabIndex = 0;
      this.cbListe.TextChanged += new System.EventHandler(this.cbListe_TextChanged);
      // 
      // btOuvrir
      // 
      this.btOuvrir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btOuvrir.Image = global::UserControls.Properties.Resources.Ouvrir22x18;
      this.btOuvrir.Location = new System.Drawing.Point(482, -1);
      this.btOuvrir.Margin = new System.Windows.Forms.Padding(0);
      this.btOuvrir.Name = "btOuvrir";
      this.btOuvrir.Size = new System.Drawing.Size(24, 23);
      this.btOuvrir.TabIndex = 2;
      this.btOuvrir.UseVisualStyleBackColor = true;
      this.btOuvrir.Click += new System.EventHandler(this.btOuvrir_Click);
      // 
      // btSupprimer
      // 
      this.btSupprimer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btSupprimer.Image = global::UserControls.Properties.Resources.Effacer32x18;
      this.btSupprimer.Location = new System.Drawing.Point(448, -1);
      this.btSupprimer.Margin = new System.Windows.Forms.Padding(0);
      this.btSupprimer.Name = "btSupprimer";
      this.btSupprimer.Size = new System.Drawing.Size(34, 23);
      this.btSupprimer.TabIndex = 1;
      this.btSupprimer.UseVisualStyleBackColor = true;
      this.btSupprimer.Click += new System.EventHandler(this.btSupprimer_Click);
      // 
      // ucListe
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.btOuvrir);
      this.Controls.Add(this.btSupprimer);
      this.Controls.Add(this.cbListe);
      this.Name = "ucListe";
      this.Size = new System.Drawing.Size(507, 22);
      this.Load += new System.EventHandler(this.ucListe_Load);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ComboBox cbListe;
    private System.Windows.Forms.Button btSupprimer;
    private System.Windows.Forms.Button btOuvrir;
  }
}
