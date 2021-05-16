using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace UserControls
{
  /*
   * Gère une liste de strings persistente présentée dans une combo box
   * La liste est sauvée en base de registre, sous-rubrique de HKCU\Software
   */
  public partial class ucListe : UserControl
  {
    public enum ShowOpen
    {
      None,
      FileBrowser,
      DirectoryBrowser
    }
    private ShowOpen showOpenButtonValue;
    public ShowOpen ShowOpenButton { get => showOpenButtonValue; set => RegleAffichageOpenButton(value); }
    public class ValueChangedEventArgs : EventArgs
    {
      public string Value;
    }
    private string key, valueName;
    public delegate void ValueChangedHandler(object sender, ValueChangedEventArgs e);
    public event ValueChangedHandler ValueChanged;

    public ucListe()
    {
      InitializeComponent();
    }
    // A appeler au démarrage de l'application
    // key = chaine débutant ou non par Software\
    // en son absence, cette racine est ajoutée.
    // Typiquement, key sera <Société>\<Produit>
    // Le nom de la valeur est constitué du nom qualifié du controle
    public void Init(string key)
    {
      cbListe.Items.Clear();
      if (!key.StartsWith(@"Software\", StringComparison.CurrentCultureIgnoreCase))
      {
        key = @"Software\" + key;
      }
      this.key = key;
      Control ctrl = this;
      valueName = Name;
      for (ctrl = ctrl.Parent; ctrl != null; ctrl = ctrl.Parent)
      {
        valueName = ctrl.Name + "." + valueName;
      }
      RegistryKey rk = Registry.CurrentUser.OpenSubKey(key);
      if (rk == null)
      {
        return;
      }
      object oListe = rk.GetValue(valueName);
      if (oListe == null)
      {
        return;
      }
      IEnumerable<string> liste = oListe as IEnumerable<string>;
      if (liste == null)
      {
        return;
      }
      liste.ToList().ForEach(s => cbListe.Items.Add(s));
      if (cbListe.Items.Count > 0)
      {
        cbListe.SelectedIndex = 0;
      }
    }
    public void Save()
    {
      string currentValue = cbListe.Text;
      if (!string.IsNullOrEmpty(currentValue) && !cbListe.Items.Contains(currentValue))
      {
        cbListe.Items.Insert(0, currentValue);
      }
      List<string> liste = cbListe.Items.Cast<string>().ToList();
      if (!string.IsNullOrEmpty(currentValue))
      {
        if (liste.Contains(currentValue))
        {
          liste.Remove(currentValue);
        }
        liste.Insert(0, currentValue);
      }

      RegistryKey rk = Registry.CurrentUser.OpenSubKey(key, true);
      if (rk == null)
      {
        rk = Registry.CurrentUser.CreateSubKey(key);
      }
      rk.SetValue(valueName, liste.ToArray());
    }

    private void btSupprimer_Click(object sender, EventArgs e)
    {
      try
      {
        cbListe.Items.Remove(cbListe.Text);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.Print(ex.ToString());
      }
      cbListe.Text = string.Empty;
    }

    private void cbListe_TextChanged(object sender, EventArgs e)
    {
      try
      {
        ValueChanged?.Invoke(this, new ValueChangedEventArgs { Value = cbListe.Text });
      }
      catch (Exception)
      {
      }
    }

    private void ucListe_Load(object sender, EventArgs e)
    {
      RegleAffichageOpenButton(showOpenButtonValue);
    }

    private void RegleAffichageOpenButton(ShowOpen value)
    {
      showOpenButtonValue = value;
      if (ShowOpenButton == ShowOpen.None)
      {
        btOuvrir.Hide();
        btSupprimer.Left = btOuvrir.Right - btSupprimer.Width;
        cbListe.Width = btSupprimer.Left - cbListe.Left;
      }
      else
      {
        btSupprimer.Left = btOuvrir.Left - btSupprimer.Width;
        cbListe.Width = btSupprimer.Left - cbListe.Left;
        btOuvrir.Show();
      }
    }

    private void btOuvrir_Click(object sender, EventArgs e)
    {
      string path = string.Empty;
      switch (ShowOpenButton)
      {
        case ShowOpen.FileBrowser:
          {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            path = dlg.FileName;
          }
          break;
        case ShowOpen.DirectoryBrowser:
          {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            path = dlg.SelectedPath;
          }
          break;
        case ShowOpen.None:
        default:
          return;
      }
      if (!cbListe.Items.Contains(path))
      {
        cbListe.Items.Insert(0, path);
      }
      cbListe.SelectedIndex = cbListe.Items.IndexOf(path);
    }

    public string Value { get => cbListe.Text; }
  }
}
