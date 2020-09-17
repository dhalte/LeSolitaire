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
using System.Xml;
using LeSolitaireLogique;

namespace LeSolitaire
{
  public partial class Form1 : Form
  {

    private void Form1_Load(object sender, EventArgs e)
    {
      ucListeFichiers.Init("danielHalte/LeSolitaire");
    }

    public Form1()
    {
      InitializeComponent();
    }

    private Logique Logique;
    private LogiqueRechercheMouvements LogiqueRechercheMouvements;
    List<Mvt> Solution;
    int IdxEtape;
    private void btRecherche_Click(object sender, EventArgs e)
    {
      if (!InitLogique(enumLogique.Logique))
      {
        return;
      }
      // MSIL, don't prefer 32 bits, <gcAllowVeryLargeObjects enabled="true" in app.config, MaxWorkingSet
      IntPtr intPtr = Process.GetCurrentProcess().MaxWorkingSet;
      intPtr = IntPtr.Add(intPtr, intPtr.ToInt32());
      Process.GetCurrentProcess().MaxWorkingSet = intPtr;

      DateTime start = DateTime.Now;
      Debug.Print($"Début recherche {start:HH:mm:ss.fff}");
      Mvt lastMvtSolution = Logique.RechercheSolution();
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
    private enum enumLogique
    {
      Logique,
      LogiqueRechercheMouvement
    }
    private bool InitLogique(enumLogique enumLogique)
    {
      string fichierSituationInitiale = ucListeFichiers.Value;
      try
      {
        if (!File.Exists(fichierSituationInitiale))
          throw new ApplicationException("Le fichier de description est invalide ou inexistant");
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
      ucListeFichiers.Save();
      Solution = null;
      SituationRaw situationInitiale;
      try
      {
        situationInitiale = Common.ChargeContenuFichierSituation(new FileInfo(fichierSituationInitiale));
      }
      catch (ApplicationException ex)
      {
        MessageBox.Show($"Erreur au chargement de {fichierSituationInitiale} : {ex.Message} ", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
      switch (enumLogique)
      {
        case enumLogique.Logique:
          Logique = new Logique(situationInitiale);
          break;
        case enumLogique.LogiqueRechercheMouvement:
          LogiqueRechercheMouvements = new LogiqueRechercheMouvements(situationInitiale);
          break;
        default:
          break;
      }
      return true;
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
      if (Logique == null || Solution == null) return;
      if (IdxEtape >= 0)
      {
        --IdxEtape;
        pbSolution.Refresh();
      }
    }

    private void btDown_Click(object sender, EventArgs e)
    {
      if (Logique == null || Solution == null) return;
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
      if (Logique == null)
      {
        return;
      }

      // Nombre de cases en largeur, en hauteur
      int NbCasesWidth = Logique.CoordonneesStock.xMax - Logique.CoordonneesStock.xMin + 1, NbCasesHeight = Logique.CoordonneesStock.yMax - Logique.CoordonneesStock.yMin + 1;
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
      Situation situation = new Situation(Logique.SituationInitiale);
      for (int idxEtape = 0; idxEtape <= IdxEtape; idxEtape++)
      {
        Mvt mvt = Solution[idxEtape];
        situation.DeplacePierre(mvt, Logique.CoordonneesStock.Etendue);
      }
      // Dessin des cases du plateau
      Image pierre = Properties.Resources.Pierre;
      Image creux = Properties.Resources.Creux;
      for (int x = Logique.CoordonneesStock.xMin; x <= Logique.CoordonneesStock.xMax; x++)
      {
        for (int y = Logique.CoordonneesStock.yMin; y <= Logique.CoordonneesStock.yMax; y++)
        {
          Coordonnee coordonnee = Logique.CoordonneesStock.GetCoordonnee(x, y);
          if (Logique.Plateau.Contains(coordonnee))
          {
            Image img = situation.Contains(coordonnee) ? pierre : creux;
            Rectangle rc = new Rectangle(margeHorizontale + (coordonnee.X - Logique.CoordonneesStock.xMin) * CaseEdge, margeVerticale + (coordonnee.Y - Logique.CoordonneesStock.yMin) * CaseEdge, CaseEdge, CaseEdge);
            g.DrawImage(img, rc);
          }
        }
      }

    }

    private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (tabMain.SelectedTab == tabRechercheManuelle)
      {
        if (!InitLogique(enumLogique.Logique))
        {
          return;
        }
        ucPlateauManuel.InitJeu(Logique.CoordonneesStock, Logique.Plateau, Logique.SituationInitiale, null, true);
      }
    }

    private LogiqueIncrementale LogiqueIncrementale;

    private void btRechercheIncrementale_Click(object sender, EventArgs e)
    {
      if (!InitLogiqueIncrementale())
      {
        return;
      }
      LogiqueIncrementale.RechercheIncrementale();
    }

    private bool InitLogiqueIncrementale()
    {
      string fichierSituationInitiale = ucListeFichiers.Value;
      try
      {
        if (!File.Exists(fichierSituationInitiale))
          throw new ApplicationException("Le fichier de description est invalide ou inexistant");
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
      ucListeFichiers.Save();
      List<(int x, int y, bool presencePierre)> situationInitiale;
      try
      {
        situationInitiale = Common.ChargeContenuFichierSituation(new FileInfo(fichierSituationInitiale));
      }
      catch (ApplicationException ex)
      {
        MessageBox.Show($"Erreur au chargement de {fichierSituationInitiale} : {ex.Message} ", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
      LogiqueIncrementale = new LogiqueIncrementale(new FileInfo(fichierSituationInitiale), 10);
      return true;
    }

    private void btRechercheMouvements_Click(object sender, EventArgs e)
    {
      if (!InitLogique(enumLogique.LogiqueRechercheMouvement))
      {
        return;
      }
      string final = @"
  ooo
 oxoox
xoxoxoo
xxxxoxx
xxxxxxx
 xxxxx
  xxx";
      Mvt solution = LogiqueRechercheMouvements.RechercheMouvements(final);
      if (solution != null)
      {

        Solution = ConvertitSolution(solution);
      }
      IdxEtape = -1;
      if (Solution == null)
      {
        MessageBox.Show("Aucune solution trouvée");
      }
      pbSolution.Refresh();
    }

  }
}
