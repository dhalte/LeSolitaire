using LeSolitaireLogique;
using LeSolitaireLogique.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UserControls
{
  public partial class ucSolutions : UserControl
  {
    private List<SolutionDetaillee> solutions;
    public ucSolutions()
    {
      InitializeComponent();
    }

    public void Init(Moteur moteur)
    {
      solutions = moteur.GetSolutionsDetaillees();
      cbListeSolutions.Items.Clear();
      for (int idxSolution = 0; idxSolution < solutions.Count; idxSolution++)
      {
        cbListeSolutions.Items.Add($"solution {idxSolution + 1}");
      }
      if (cbListeSolutions.Items.Count>0)
      {
        cbListeSolutions.SelectedIndex = 0;
      }
    }

    private void cbListeSolutions_SelectedIndexChanged(object sender, EventArgs e)
    {
      int idx = cbListeSolutions.SelectedIndex;
      if (idx<0)
      {
        ucPlateau.Init(null);
      }
      else
      {
        ucPlateau.Init(solutions[idx]);
      }
    }
  }
}
