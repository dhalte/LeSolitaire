using LeSolitaireLogique;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LeSolitaire
{
  static class Program
  {
    /// <summary>
    /// Point d'entrée principal de l'application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      // Tests.EvaluePertinenceHashage();
      // Tests.BuildBTree();
      // Tests.TesteRechercheSituationInEfsorted();
      // string fichierLog = Path.Combine(Application.StartupPath, $"LeSolitaire {DateTime.Now:yyyy-MM-dd HH.mm.ss}.log");
      // TimeSpan timeSpan = new TimeSpan(0, 0, 10);
      // SuiviMemoire suiviMemoire = new SuiviMemoire(fichierLog, timeSpan);
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new frmMain());
    }
 }
}
