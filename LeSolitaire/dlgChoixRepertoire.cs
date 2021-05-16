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
  public partial class dlgChoixRepertoire : Form
  {
    public dlgChoixRepertoire()
    {
      InitializeComponent();
    }

    private void btOK_Click(object sender, EventArgs e)
    {
      ucListeRepertoires.Save();
      this.DialogResult = DialogResult.OK;
    }

    private void dlgChoixRepertoire_Load(object sender, EventArgs e)
    {
      ucListeRepertoires.Init(ActionControle.KeyListe);
    }
    public string Value => ucListeRepertoires.Value;
  }
}
