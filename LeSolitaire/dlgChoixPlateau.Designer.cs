﻿
namespace LeSolitaire
{
  partial class dlgChoixPlateau
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.label1 = new System.Windows.Forms.Label();
      this.ucListeDepuisUnFichierTexte = new UserControls.ucListe();
      this.btCancel = new System.Windows.Forms.Button();
      this.btOK = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(112, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Depuis un fichier texte";
      // 
      // ucListeDepuisUnFichierTexte
      // 
      this.ucListeDepuisUnFichierTexte.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.ucListeDepuisUnFichierTexte.BackColor = System.Drawing.SystemColors.Control;
      this.ucListeDepuisUnFichierTexte.Location = new System.Drawing.Point(15, 25);
      this.ucListeDepuisUnFichierTexte.Name = "ucListeDepuisUnFichierTexte";
      this.ucListeDepuisUnFichierTexte.ShowOpenButton = UserControls.ucListe.ShowOpen.FileBrowser;
      this.ucListeDepuisUnFichierTexte.Size = new System.Drawing.Size(507, 22);
      this.ucListeDepuisUnFichierTexte.TabIndex = 1;
      // 
      // btCancel
      // 
      this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btCancel.Location = new System.Drawing.Point(365, 65);
      this.btCancel.Name = "btCancel";
      this.btCancel.Size = new System.Drawing.Size(75, 23);
      this.btCancel.TabIndex = 3;
      this.btCancel.Text = "&Cancel";
      this.btCancel.UseVisualStyleBackColor = true;
      // 
      // btOK
      // 
      this.btOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btOK.Location = new System.Drawing.Point(447, 65);
      this.btOK.Name = "btOK";
      this.btOK.Size = new System.Drawing.Size(75, 23);
      this.btOK.TabIndex = 4;
      this.btOK.Text = "&OK";
      this.btOK.UseVisualStyleBackColor = true;
      this.btOK.Click += new System.EventHandler(this.btOK_Click);
      // 
      // dlgChoixPlateau
      // 
      this.AcceptButton = this.btOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btCancel;
      this.ClientSize = new System.Drawing.Size(535, 97);
      this.Controls.Add(this.btOK);
      this.Controls.Add(this.btCancel);
      this.Controls.Add(this.ucListeDepuisUnFichierTexte);
      this.Controls.Add(this.label1);
      this.MaximizeBox = false;
      this.MaximumSize = new System.Drawing.Size(1024, 136);
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(200, 136);
      this.Name = "dlgChoixPlateau";
      this.Text = "Choix du plateau";
      this.Load += new System.EventHandler(this.dlgChoixPlateau_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private UserControls.ucListe ucListeDepuisUnFichierTexte;
    private System.Windows.Forms.Button btCancel;
    private System.Windows.Forms.Button btOK;
  }
}