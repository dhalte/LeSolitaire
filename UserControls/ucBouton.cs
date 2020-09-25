using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace UserControls
{
  public partial class ucBouton : UserControl
  {
    public ucBouton()
    {
      InitializeComponent();
    }
    private Bitmap imgBoutons = Properties.Resources.Controles128x32;

    public ucBoutonFx ucBoutonFx { get; set; }
    // Le parent définit cet état, qui conditionne la réponse du bouton aux actions de la souris
    private bool _IsActif;
    public bool IsActif { get => _IsActif; set { _IsActif = value; pbBouton.Refresh(); } }
    // bouton souris enfoncé, initialement sur le bouton, mais la souris peut avoir bougé depuis
    private bool _IsDown;
    private bool IsDown { get => _IsDown; set { _IsDown = value; pbBouton.Refresh(); } }
    // La souris survole le bouton
    private bool _IsOver;
    private bool IsOver { get => _IsOver; set { _IsOver = value; pbBouton.Refresh(); } }

    // Le parent traite cet événement
    public event EventHandler OnClic;
    // Permet au parent de déclencher par programme l'événement OnClic
    public void SimuleClic()
    {
      // Quel que soit l'état actuel, si le parent le demande, on le lui fournit.
      OnClic?.Invoke(this, EventArgs.Empty);
    }

    private void pbBouton_MouseDown(object sender, MouseEventArgs e)
    {
      IsDown = true;
    }

    private void pbBouton_MouseEnter(object sender, EventArgs e)
    {
      IsOver = true;
    }

    private void pbBouton_MouseLeave(object sender, EventArgs e)
    {
      IsOver = false;
    }

    private void pbBouton_MouseUp(object sender, MouseEventArgs e)
    {
      if (IsActif && IsDown && IsOver)
      {
        OnClic?.Invoke(this, EventArgs.Empty);
      }
      IsDown = false;
    }

    // Quand on enfonce le bouton de la souris, puis bouge la souris, Mouse_Leave n'est plus déclenché.
    private void pbBouton_MouseMove(object sender, MouseEventArgs e)
    {
      bool b = pbBouton.ClientRectangle.Contains(e.Location);
      if (b != IsOver) IsOver = b;
    }

    private void pbBouton_Paint(object sender, PaintEventArgs e)
    {
      Color bc = IsActif ? IsOver ? Color.LightBlue : Color.White : Color.LightGray;
      Graphics g = e.Graphics;
      g.FillRectangle(new SolidBrush(bc), pbBouton.ClientRectangle);
      // stretch de l'image
      Rectangle rcDest = pbBouton.ClientRectangle;
      int w1 = imgBoutons.Width / 4;
      int h1 = imgBoutons.Height;
      Rectangle rcSrc = new Rectangle((int)ucBoutonFx * w1, 0, w1, h1);
      if (IsActif && IsDown && IsOver)
      {
        rcDest.Offset(2, 2);
      }
      g.DrawImage(imgBoutons, rcDest, rcSrc, GraphicsUnit.Pixel);
    }
  }
}
