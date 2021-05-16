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
  public partial class ucSuivi : UserControl
  {
    public ucSuivi()
    {
      InitializeComponent();
    }

    private void mnuClear_Click(object sender, EventArgs e)
    {
      Clear();
    }
    delegate void delegateClear();
    public void Clear()
    {
      if (this.InvokeRequired)
      {
        this.Invoke(new delegateClear(Clear));
        return;
      }
      tbSuivi.Clear();
    }
    private enum Niveau
    {
      Info,
      Warning,
      Erreur
    }
    delegate void delegateAddMsg(string msg, Niveau niveau);
    public void AddInfo(string msg)
    {
      AddMsg(msg, Niveau.Info);
    }
    public void AddWarning(string msg)
    {
      AddMsg(msg, Niveau.Warning);
    }
    public void AddErreur(string msg)
    {
      AddMsg(msg, Niveau.Erreur);
    }
    private void AddMsg(string msg, Niveau niveau)
    {
      msg = $"{DateTime.Now:HH:mm:ss} {msg}{Environment.NewLine}";
      if (this.InvokeRequired)
      {
        this.Invoke(new delegateAddMsg(AddMsg2), msg, niveau);
      }
      else
      {
        AddMsg2(msg, niveau);
      }
    }
    private void AddMsg2(string msg, Niveau niveau)
    {
      // Placer le point d'insertion en fin de texte
      tbSuivi.SelectionLength = 0;
      tbSuivi.SelectionStart = tbSuivi.TextLength;
      // Etrange : on définit d'abord la couleur qu'on va donner au texte ajouté, 
      // puis l'instruction [.SelectedText = "xxx"] remplace la sélection courante par le texte fourni, et lui donne la couleur qu'on a définie.
      Color color;
      switch (niveau)
      {
        case Niveau.Info:
          color = Color.Black;
          break;
        case Niveau.Warning:
          color = Color.Orange;
          break;
        case Niveau.Erreur:
        default:
          color = Color.Red;
          break;
      }
      tbSuivi.SelectionColor = color;
      tbSuivi.SelectedText = msg;
    }
  }
}
