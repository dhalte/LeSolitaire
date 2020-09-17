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
    private CoordonneesStock CoordonneesStock;
    private int NbCasesEnLargeur;
    private int NbCasesEnHauteur;
    private int EdgeCase;
    // PlateauJeu et SituationInitiales sont null avant d'être définis.
    private Plateau PlateauJeu;
    private Situation SituationInitiale;
    private Situation SituationActuelle;
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
      if (CoordonneesStock == null)
      {
        return;
      }
      PlateauInteraction = new PlateauInteraction();
      NbCasesEnLargeur = CoordonneesStock.xMax - CoordonneesStock.xMin + 1;
      NbCasesEnHauteur = CoordonneesStock.yMax - CoordonneesStock.yMin + 1;
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
      for (int x = CoordonneesStock.xMin; x <= CoordonneesStock.xMax; x++)
      {
        for (int y = CoordonneesStock.yMin; y <= CoordonneesStock.yMax; y++)
        {
          Coordonnee coordonnee = CoordonneesStock.GetCoordonnee(x, y);
          if (PlateauJeu.Contains(coordonnee))
          {
            Image img = SituationActuelle.Contains(coordonnee) ? pierre : creux;
            Rectangle rc = new Rectangle((coordonnee.X - CoordonneesStock.xMin) * EdgeCase, (coordonnee.Y - CoordonneesStock.yMin) * EdgeCase, EdgeCase, EdgeCase);
            g.DrawImage(img, rc);
          }
        }
      }

    }
    private void ReconstitueSituation()
    {
      SituationActuelle = new Situation(SituationInitiale);
      for (int idxEtape = 0; idxEtape <= IdxEtapeActuelle; idxEtape++)
      {
        Mvt mvt = Mouvements[idxEtape];
        SituationActuelle.DeplacePierre(mvt, CoordonneesStock.Etendue);
      }
    }
    public void InitJeu(CoordonneesStock coordonneesStock, Plateau plateau, Situation situationInitiale, List<Mvt> mouvements, bool bManuel)
    {
      CoordonneesStock = coordonneesStock;
      PlateauJeu = plateau;
      SituationInitiale = situationInitiale;
      SituationActuelle = new Situation(situationInitiale);
      PlateauInteraction = new PlateauInteraction();
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
          ReconstitueSituation();
          pbPlateau.Refresh();
          break;
        case ucBoutonFx.Left:
          if (IdxEtapeActuelle > -1)
          {
            IdxEtapeActuelle--;
            ReconstitueSituation();
            pbPlateau.Refresh();
          }
          break;
        case ucBoutonFx.Right:
          if (IdxEtapeActuelle < Mouvements.Count - 1)
          {
            IdxEtapeActuelle++;
            ReconstitueSituation();
            pbPlateau.Refresh();
          }
          break;
        case ucBoutonFx.RightRight:
          IdxEtapeActuelle = Mouvements.Count - 1;
          ReconstitueSituation();
          pbPlateau.Refresh();
          break;
        default:
          break;
      }
    }

    private PlateauInteraction PlateauInteraction;
    private void pbPlateau_MouseDown(object sender, MouseEventArgs e)
    {
      int x = e.Location.X / EdgeCase;
      int y = e.Location.Y / EdgeCase;
      bool bCoordonneeOK = CoordonneesStock.Contains(x, y);
      Coordonnee coordonnee = bCoordonneeOK ? CoordonneesStock.GetCoordonnee(x, y) : null;
      bCoordonneeOK = bCoordonneeOK && PlateauJeu.Contains(coordonnee);
      bool bPierre = bCoordonneeOK && SituationActuelle.Contains(coordonnee);
      bool bVide = bCoordonneeOK && !bPierre;
      if (bPierre)
      {
        PlateauInteraction.IsPierreSelectionnee = true;
        PlateauInteraction.PierreSelectionnee = coordonnee; 
      }
    }

    private void pbPlateau_MouseUp(object sender, MouseEventArgs e)
    {

    }

    private void pbPlateau_MouseMove(object sender, MouseEventArgs e)
    {

    }

  }
}
