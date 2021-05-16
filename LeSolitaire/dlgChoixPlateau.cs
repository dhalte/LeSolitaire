using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LeSolitaire
{
  public partial class dlgChoixPlateau : Form
  {
    public dlgChoixPlateau()
    {
      InitializeComponent();
    }

    private void btOK_Click(object sender, EventArgs e)
    {
      ucListeDepuisUnFichierTexte.Save();
      this.DialogResult = DialogResult.OK;
    }

    private void dlgChoixPlateau_Load(object sender, EventArgs e)
    {
      ucListeDepuisUnFichierTexte.Init(ActionControle.KeyListe);
    }
    public string Value => ucListeDepuisUnFichierTexte.Value;
  }
}
