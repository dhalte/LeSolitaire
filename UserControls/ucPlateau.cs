using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LeSolitaireLogiqueV0;

namespace UserControls
{
  public partial class ucPlateau : UserControl
  {
    private Pilote Pilote;
    private Plateau Plateau;
    private Solution Solution;
    private SituationEtude SituationActuelle;

    // IdxEtapeActuelle débute à -1, indication que l'on affiche la situation initiale.
    private int IdxEtapeActuelle;

    int NbCasesEnLargeur;
    int NbCasesEnHauteur;
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
      if (Pilote == null)
      {
        return;
      }

      NbCasesEnLargeur = Plateau.Etendue.Largeur;
      NbCasesEnHauteur = Plateau.Etendue.Hauteur;
      if (NbCasesEnLargeur == 0 || NbCasesEnHauteur == 0)
      {
        return;
      }
      int largeurCase = Width / NbCasesEnLargeur;
      System.Diagnostics.Debug.Print($"Height {Height} ucBoutons.Top {ucBoutons.Top} ucBoutons.Enabled {ucBoutons.Enabled}");
      int hauteurCase = Height / NbCasesEnHauteur;
      // cases carrées
      EdgeCase = largeurCase < hauteurCase ? largeurCase : hauteurCase;

      int margePlateauH = (Width - EdgeCase * NbCasesEnLargeur) / 2;
      int margePlateauV = (Height - EdgeCase * NbCasesEnHauteur) / 2;
      pbPlateau.Bounds = new Rectangle(margePlateauH, margePlateauV, EdgeCase * NbCasesEnLargeur, EdgeCase * NbCasesEnHauteur);
      pbPlateau.Refresh();
    }

    private void pbPlateau_Paint(object sender, PaintEventArgs e)
    {
      Graphics g = e.Graphics;
      g.FillRectangle(new SolidBrush(pbPlateau.BackColor), pbPlateau.ClientRectangle);
      if (Solution == null)
      {
        return;
      }
      // Dessin des cases du plateau
      Image pierre = Properties.Resources.Pierre;
      Image creux = Properties.Resources.Creux;

      for (int y = 0; y < NbCasesEnHauteur; y++)
      {
        for (int x = 0; x < NbCasesEnLargeur; x++)
        {
          int idxCase = Plateau.Etendue.FromXY(x, y);
          if (Plateau.Contains(x, y))
          {
            Image img = SituationActuelle.Pierres[idxCase] ? pierre : creux;
            Rectangle rc = new Rectangle(x * EdgeCase, y * EdgeCase, EdgeCase, EdgeCase);
            g.DrawImage(img, rc);
          }
        }
      }

    }
    private void ReconstitueSituation()
    {
      SituationActuelle.ChargeSituation(Solution.SituationInitiale);
      for (int idxEtape = 0; idxEtape <= IdxEtapeActuelle; idxEtape++)
      {
        SolutionMouvement mvt = Solution.Mouvements[idxEtape];
        SituationActuelle.EffectueMouvement((mvt.IdxDepart, mvt.IdxSaut, mvt.IdxArrivee(Plateau.Etendue)));
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
          if (IdxEtapeActuelle < Solution.Mouvements.Count - 1)
          {
            IdxEtapeActuelle++;
          }
          break;
        case ucBoutonFx.RightRight:
          IdxEtapeActuelle = Solution.Mouvements.Count - 1;
          break;
        default:
          break;
      }
      ReconstitueSituation();
      ucBoutons.Set(true, -1, Solution.Mouvements.Count - 1, IdxEtapeActuelle);
      pbPlateau.Refresh();
    }

    internal void Init(Pilote pilote, int idxSolutionChoisie)
    {
      Pilote = pilote;
      Plateau = new Plateau(pilote.PlateauRaw);
      if (idxSolutionChoisie < 0)
      {
        Solution = null;
        Refresh();
      }
      else
      {
        ChangeSolution(idxSolutionChoisie);
      }
    }

    public void ChangeSolution(int idxSolutionChoisie)
    {
      Solution = Pilote.Solutions[idxSolutionChoisie];
      if (Solution.SituationInitiale == null)
      {
        Solution.SituationInitiale = new Situation(Plateau.Etendue, Solution.SituationInitialeRaw);
      }
      IdxEtapeActuelle = -1;
      SituationActuelle = new SituationEtude(Plateau);
      SituationActuelle.ChargeSituation(Solution.SituationInitiale);
      ucBoutons.Set(true, -1, Solution.Mouvements.Count - 1, IdxEtapeActuelle);
      ResizePlateau();
      Refresh();
    }
  }
}
