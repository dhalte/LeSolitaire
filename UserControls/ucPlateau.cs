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

namespace UserControls
{
  public partial class ucPlateau : UserControl
  {
    private int NbCasesEnLargeur;
    private int NbCasesEnHauteur;
    private int EdgeCase;
    // PlateauJeu et SituationInitiales sont null avant d'être définis.
    private Plateau PlateauJeu;
    private SituationEtude SituationInitiale;
    private SituationEtude SituationActuelle;
    // Soit initialisé à Count=0 si on est en mode manuel, soit à la liste des mouvements de la solution à afficher.
    private List<Mvt> Mouvements;
    // IdxEtapeActuelle débute à -1, indication que l'on affiche la situation initiale.
    private int IdxEtapeActuelle;
    private bool ModeManuel;
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
      if (PlateauJeu == null)
      {
        return;
      }
      NbCasesEnLargeur = PlateauJeu.Etendue.Largeur;
      NbCasesEnHauteur = PlateauJeu.Etendue.Hauteur;
      if (NbCasesEnLargeur == 0 || NbCasesEnHauteur == 0)
      {
        return;
      }
      int largeurCase = Width / NbCasesEnLargeur;
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
      if (PlateauJeu == null || SituationInitiale == null)
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
          int idxCase = PlateauJeu.Etendue.FromXY(x, y);
          if (PlateauJeu.Contains(x, y))
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
      SituationActuelle = new SituationEtude(PlateauJeu);
      for (int idxEtape = 0; idxEtape <= IdxEtapeActuelle; idxEtape++)
      {
        Mvt mvt = Mouvements[idxEtape];
        SituationActuelle.EffectueMouvement((mvt.Depart, mvt.Saut, mvt.Arrivee));
      }
    }
    public void InitJeu(Plateau plateau, Situation situationInitiale, List<Mvt> mouvements, bool bManuel)
    {

      PlateauJeu = plateau;
      SituationInitiale = new SituationEtude(plateau);
      SituationInitiale.ChargeSituation(situationInitiale);
      SituationActuelle = new SituationEtude(plateau);

      if (mouvements == null)
      {
        Mouvements = new List<Mvt>();
      }
      else
      {
        Mouvements = new List<Mvt>(mouvements);
      }
      IdxEtapeActuelle = -1;
      ModeManuel = bManuel;
      ResizePlateau();
      pbPlateau.Refresh();
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
          if (IdxEtapeActuelle < Mouvements.Count - 1)
          {
            IdxEtapeActuelle++;
          }
          break;
        case ucBoutonFx.RightRight:
          IdxEtapeActuelle = Mouvements.Count - 1;
          break;
        default:
          break;
      }
      ReconstitueSituation();
      pbPlateau.Refresh();
    }

  }
}
