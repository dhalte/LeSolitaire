using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LeSolitaire
{
  public partial class Form1 : Form
  {
    private const string fichierSituationInitiale = @"C:\Users\halte\reposDivers\LeSolitaire\Jeux\classique.txt";
    public Form1()
    {
      InitializeComponent();
    }
    private Logique logique;
    List<Mvt> Solution;
    int IdxEtape;
    private void btRecherche_Click(object sender, EventArgs e)
    {
      DateTime start = DateTime.Now;
      Debug.Print($"Début recherche {start:HH:mm:ss.fff}");
      Solution = null;
      List<(int x, int y, bool presencePierre)> situationInitiale = null;
      try
      {
        situationInitiale = Common.ChargeContenuFichierSituationInitiale(new FileInfo(fichierSituationInitiale));
      }
      catch (ApplicationException ex)
      {
        MessageBox.Show($"Erreur au chargement de {fichierSituationInitiale} : {ex.Message} ");
        return;
      }
      logique = new Logique(situationInitiale);
      Mvt lastMvtSolution = logique.RechercheSolution();
      DateTime stop = DateTime.Now;
      Debug.Print($"Fin recherche {stop:HH:mm:ss.fff}");
      Debug.Print($"Durée {(stop - start)}");

      if (lastMvtSolution != null)
      {
        Solution = ConvertitSolution(lastMvtSolution);
      }
      IdxEtape = -1;
      if (Solution == null)
      {
        MessageBox.Show("Aucune solution trouvée");
      }
      pbSolution.Refresh();
    }

    private List<Mvt> ConvertitSolution(Mvt solution)
    {
      List<Mvt> solutionConvertie = new List<Mvt>();
      Mvt mvt = solution;
      do
      {
        solutionConvertie.Add(mvt);
        mvt = mvt.Parent;
      } while (mvt != null);
      solutionConvertie.Reverse();

      return solutionConvertie;
    }

    private void btUp_Click(object sender, EventArgs e)
    {
      if (logique == null || Solution == null) return;
      if (IdxEtape >= 0)
      {
        --IdxEtape;
        pbSolution.Refresh();
      }
    }

    private void btDown_Click(object sender, EventArgs e)
    {
      if (logique == null || Solution == null) return;
      if (IdxEtape < Solution.Count - 1)
      {
        ++IdxEtape;
        pbSolution.Refresh();
      }
    }

    private void pbSolution_Paint(object sender, PaintEventArgs e)
    {
      Graphics g = e.Graphics;
      g.FillRectangle(new SolidBrush(pbSolution.BackColor), pbSolution.ClientRectangle);
      if (logique == null)
      {
        return;
      }

      // Nombre de cases en largeur, en hauteur
      int NbCasesWidth = Coordonnees.xMax - Coordonnees.xMin + 1, NbCasesHeight = Coordonnees.yMax - Coordonnees.yMin + 1;
      // Largeur suggérée d'une case, hauteur suggérée d'une case
      int CaseWidth = pbSolution.Width / NbCasesWidth;
      int CaseHeight = pbSolution.Height / NbCasesHeight;
      // Largeur,Hauteur adoptée d'une case
      int CaseEdge = CaseWidth;
      if (CaseHeight < CaseWidth) CaseEdge = CaseHeight;
      // Marges pour centrer le plateau (l'une d'elles est 0)
      int margeHorizontale = (pbSolution.Width - NbCasesWidth * CaseEdge) / 2;
      int margeVerticale = (pbSolution.Height - NbCasesHeight * CaseEdge) / 2;
      // Reconstitution de la situation à l'étape IdxEtape
      Situation situation = new Situation(logique.SituationInitiale);
      for (int idxEtape = 0; idxEtape <= IdxEtape; idxEtape++)
      {
        Mvt mvt = Solution[idxEtape];
        situation.DeplacePierre(mvt.Depart, mvt.Saut, mvt.Arrivee);
      }
      // Dessin des cases du plateau
      Image pierre = Properties.Resources.Pierre;
      Image creux = Properties.Resources.Creux;
      for (int x = Coordonnees.xMin; x <= Coordonnees.xMax; x++)
      {
        for (int y = Coordonnees.yMin; y <= Coordonnees.yMax; y++)
        {
          Coordonnee coordonnee = Coordonnees.GetCoordonnee(x, y);
          if (logique.Plateau.Contains(coordonnee))
          {
            Image img = situation.Contains(coordonnee) ? pierre : creux;
            Rectangle rc = new Rectangle(margeHorizontale + (coordonnee.X - Coordonnees.xMin) * CaseEdge, margeVerticale + (coordonnee.Y - Coordonnees.yMin) * CaseEdge, CaseEdge, CaseEdge);
            g.DrawImage(img, rc);
          }
        }
      }
      // Dessin d'une flèche qui montre le prochain mouvement
      //if (IdxEtape < Solution.Count - 1)
      //{
      //  Mvt prochain = Solution[IdxEtape + 1];
      //  Image fleche = Properties.Resources.Fleche;

      //}
    }
  }
}
