using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LeSolitaireLogique;
using LeSolitaireLogique.Services;

namespace UserControls
{
  public partial class ucPlateau : UserControl
  {

    // IdxEtapeActuelle débute à -1, indication que l'on affiche la situation initiale.
    private int IdxEtapeActuelle;
    private int[,] SituationActuelle;

    int EdgeCase;

    public ucPlateau()
    {
      InitializeComponent();
    }

    private void ucPlateau_Load(object sender, EventArgs e)
    {
      ResizePlateau();
    }

    private void ucPlateau_Resize(object sender, EventArgs e)
    {
      ResizePlateau();
    }

    private void ResizePlateau()
    {
      if (SolutionDetaillee == null)
      {
        return;
      }

      int largeurCase = Width / SolutionDetaillee.Dimensions.Width;
      System.Diagnostics.Debug.Print($"Height {Height} ucBoutons.Top {ucBoutons.Top} ucBoutons.Enabled {ucBoutons.Enabled}");
      int hauteurCase = Height / SolutionDetaillee.Dimensions.Height;
      // cases carrées
      EdgeCase = largeurCase < hauteurCase ? largeurCase : hauteurCase;

      int margePlateauH = (Width - EdgeCase * SolutionDetaillee.Dimensions.Width) / 2;
      int margePlateauV = (Height - EdgeCase * SolutionDetaillee.Dimensions.Height) / 2;
      pbPlateau.Bounds = new Rectangle(margePlateauH, margePlateauV, EdgeCase * SolutionDetaillee.Dimensions.Width, EdgeCase * SolutionDetaillee.Dimensions.Height);
      pbPlateau.Refresh();
    }

    SolutionDetaillee SolutionDetaillee;
    internal void Init(SolutionDetaillee solutionDetaillee)
    {
      SolutionDetaillee = solutionDetaillee;
      IdxEtapeActuelle = -1;
      ReconstitueSituation();
      if (solutionDetaillee == null)
      {
        ucBoutons.Set(false, -1, 0, IdxEtapeActuelle);
      }
      else
      {
        ucBoutons.Set(true, -1, SolutionDetaillee.NbMouvements - 1, IdxEtapeActuelle);
      }
      Refresh();
    }

    private void pbPlateau_Paint(object sender, PaintEventArgs e)
    {
      Graphics g = e.Graphics;
      g.FillRectangle(new SolidBrush(pbPlateau.BackColor), pbPlateau.ClientRectangle);
      if (SolutionDetaillee == null)
      {
        return;
      }
      // Dessin des cases du plateau
      Image pierre = Properties.Resources.Pierre;
      Image creux = Properties.Resources.Creux;

      for (int y = 0; y < SolutionDetaillee.Dimensions.Height; y++)
      {
        for (int x = 0; x < SolutionDetaillee.Dimensions.Width; x++)
        {
          if (SituationActuelle[x, y] >= 0)
          {
            Image img = SituationActuelle[x, y] == 1 ? pierre : creux;
            Rectangle rc = new Rectangle(x * EdgeCase, y * EdgeCase, EdgeCase, EdgeCase);
            g.DrawImage(img, rc);
          }
        }
      }

    }
    private void ReconstitueSituation()
    {
      if (SolutionDetaillee == null)
      {
        SituationActuelle = null;
      }
      else
      {
        SituationActuelle = SolutionDetaillee.CalculePlateau(IdxEtapeActuelle);
      }
    }

    private void ucBoutons_OnClic(object sender, ucBoutonEventArgs e)
    {
      switch (e.BoutonFx)
      {
        case ucBoutonFx.LeftLeft:
          IdxEtapeActuelle = -1;
          break;
        case ucBoutonFx.Left:
          if (IdxEtapeActuelle > -1)
          {
            IdxEtapeActuelle--;
          }
          break;
        case ucBoutonFx.Right:
          if (IdxEtapeActuelle < SolutionDetaillee.NbMouvements - 1)
          {
            IdxEtapeActuelle++;
          }
          break;
        case ucBoutonFx.RightRight:
          IdxEtapeActuelle = SolutionDetaillee.NbMouvements - 1;
          break;
        default:
          break;
      }
      ReconstitueSituation();
      ucBoutons.Set(true, -1, SolutionDetaillee.NbMouvements - 1, IdxEtapeActuelle);
      pbPlateau.Refresh();
    }
  }
}
