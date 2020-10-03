using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UserControls
{
  public partial class ucBoutons : UserControl
  {
    // Gestion de 4 boutons |<   <   >   >|
    // chaque bouton est un ucBouton (qui contient essentiellement une picture box)
    // chaque ucBouton affiche une portion de l'image Controles128x32.png
    // 
    private bool IsActif;
    private int Min;
    private int Max;
    private int Current;
    public ucBoutons()
    {
      InitializeComponent();
    }
    public void Set(bool bActif, int xMin, int xMax, int xInit)
    {
      IsActif = bActif;
      Min = xMin;
      Max = xMax < xMin ? xMin : xMax;
      Current = xInit < Min ? Min : xInit > Max ? Max : xInit;
      BoutonLeftLeft.IsActif = BoutonLeft.IsActif = IsActif && Current > Min;
      BoutonRightRight.IsActif = BoutonRight.IsActif = IsActif && Current < Max;
      if (IsActif)
      {
        lbl.Text = $"{Current - xMin}/{xMax - xMin}";
      }
      else
      {
        lbl.Text = string.Empty;
      }
    }

    [Browsable(true)]
    public event EventHandler<ucBoutonEventArgs> OnClic;

    private void BoutonOnClic(object sender, EventArgs e)
    {
      ucBoutonFx ucBoutonFx;
      if (sender == BoutonLeftLeft) ucBoutonFx = ucBoutonFx.LeftLeft;
      else if (sender == BoutonLeft) ucBoutonFx = ucBoutonFx.Left;
      else if (sender == BoutonRight) ucBoutonFx = ucBoutonFx.Right;
      else ucBoutonFx = ucBoutonFx.RightRight;
      OnClic?.Invoke(this, new ucBoutonEventArgs(ucBoutonFx));
    }

  }
}
