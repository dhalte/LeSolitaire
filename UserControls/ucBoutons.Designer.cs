namespace UserControls
{
  partial class ucBoutons
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
      this.tbl = new System.Windows.Forms.TableLayoutPanel();
      this.lbl = new System.Windows.Forms.Label();
      this.BoutonLeftLeft = new UserControls.ucBouton();
      this.BoutonLeft = new UserControls.ucBouton();
      this.BoutonRight = new UserControls.ucBouton();
      this.BoutonRightRight = new UserControls.ucBouton();
      this.tbl.SuspendLayout();
      this.SuspendLayout();
      // 
      // tbl
      // 
      this.tbl.ColumnCount = 5;
      this.tbl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
      this.tbl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
      this.tbl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
      this.tbl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
      this.tbl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
      this.tbl.Controls.Add(this.BoutonLeftLeft, 0, 0);
      this.tbl.Controls.Add(this.BoutonLeft, 1, 0);
      this.tbl.Controls.Add(this.BoutonRight, 2, 0);
      this.tbl.Controls.Add(this.BoutonRightRight, 3, 0);
      this.tbl.Controls.Add(this.lbl, 4, 0);
      this.tbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tbl.Location = new System.Drawing.Point(0, 0);
      this.tbl.Name = "tbl";
      this.tbl.RowCount = 1;
      this.tbl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tbl.Size = new System.Drawing.Size(192, 32);
      this.tbl.TabIndex = 0;
      // 
      // lbl
      // 
      this.lbl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lbl.Location = new System.Drawing.Point(131, 0);
      this.lbl.Name = "lbl";
      this.lbl.Size = new System.Drawing.Size(58, 32);
      this.lbl.TabIndex = 4;
      this.lbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // LeftLeft
      // 
      this.BoutonLeftLeft.Dock = System.Windows.Forms.DockStyle.Fill;
      this.BoutonLeftLeft.IsActif = false;
      this.BoutonLeftLeft.Location = new System.Drawing.Point(0, 0);
      this.BoutonLeftLeft.Margin = new System.Windows.Forms.Padding(0);
      this.BoutonLeftLeft.Name = "LeftLeft";
      this.BoutonLeftLeft.Size = new System.Drawing.Size(32, 32);
      this.BoutonLeftLeft.TabIndex = 0;
      this.BoutonLeftLeft.ucBoutonFx = UserControls.ucBoutonFx.LeftLeft;
      this.BoutonLeftLeft.OnClic += new System.EventHandler(this.BoutonOnClic);
      // 
      // Left
      // 
      this.BoutonLeft.Dock = System.Windows.Forms.DockStyle.Fill;
      this.BoutonLeft.IsActif = false;
      this.BoutonLeft.Location = new System.Drawing.Point(32, 0);
      this.BoutonLeft.Margin = new System.Windows.Forms.Padding(0);
      this.BoutonLeft.Name = "Left";
      this.BoutonLeft.Size = new System.Drawing.Size(32, 32);
      this.BoutonLeft.TabIndex = 1;
      this.BoutonLeft.ucBoutonFx = UserControls.ucBoutonFx.Left;
      this.BoutonLeft.OnClic += new System.EventHandler(this.BoutonOnClic);
      // 
      // Right
      // 
      this.BoutonRight.Dock = System.Windows.Forms.DockStyle.Fill;
      this.BoutonRight.IsActif = false;
      this.BoutonRight.Location = new System.Drawing.Point(64, 0);
      this.BoutonRight.Margin = new System.Windows.Forms.Padding(0);
      this.BoutonRight.Name = "Right";
      this.BoutonRight.Size = new System.Drawing.Size(32, 32);
      this.BoutonRight.TabIndex = 2;
      this.BoutonRight.ucBoutonFx = UserControls.ucBoutonFx.Right;
      this.BoutonRight.OnClic += new System.EventHandler(this.BoutonOnClic);
      // 
      // RightRight
      // 
      this.BoutonRightRight.Dock = System.Windows.Forms.DockStyle.Fill;
      this.BoutonRightRight.IsActif = false;
      this.BoutonRightRight.Location = new System.Drawing.Point(96, 0);
      this.BoutonRightRight.Margin = new System.Windows.Forms.Padding(0);
      this.BoutonRightRight.Name = "RightRight";
      this.BoutonRightRight.Size = new System.Drawing.Size(32, 32);
      this.BoutonRightRight.TabIndex = 3;
      this.BoutonRightRight.ucBoutonFx = UserControls.ucBoutonFx.RightRight;
      this.BoutonRightRight.OnClic += new System.EventHandler(this.BoutonOnClic);
      // 
      // ucBoutons
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tbl);
      this.Name = "ucBoutons";
      this.Size = new System.Drawing.Size(192, 32);
      this.tbl.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tbl;
    private ucBouton BoutonLeftLeft;
    private ucBouton BoutonLeft;
    private ucBouton BoutonRight;
    private ucBouton BoutonRightRight;
    private System.Windows.Forms.Label lbl;
  }
}
