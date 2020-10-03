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
      // EvalueHashage();
      // string fichierLog = Path.Combine(Application.StartupPath, $"LeSolitaire {DateTime.Now:yyyy-MM-dd HH.mm.ss}.log");
      // TimeSpan timeSpan = new TimeSpan(0, 0, 10);
      // SuiviMemoire suiviMemoire = new SuiviMemoire(fichierLog, timeSpan);
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new frmMain());
    }
    public static void EvalueHashage()
    {
      string fileName = @"C:\Users\halte\reposDivers\LeSolitaire\Jeux\Plateau Français\EF.dat";
      // il y a 1 pierre dans une SG, 13 mouvements qui séparent une SF d'une SG, donc 14 pierres dans une SF.
      int NbPierres = 14;
      byte[] buffer = new byte[NbPierres];
      Dictionary<int, int> stats = new Dictionary<int, int>();
      using (FileStream EFdat = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      {
        long EFlen = EFdat.Length;
        Debug.Assert(EFlen % NbPierres == 0);
        for (; ; )
        {
          int n = EFdat.Read(buffer, 0, NbPierres);
          if (n == 0)
          {
            break;
          }
          Debug.Assert(n == NbPierres);
          Situation situation = new Situation(buffer);
          int hashcode = situation.GetHashCode();
          if (!stats.ContainsKey(hashcode))
          {
            stats.Add(hashcode, 0);
          }
          stats[hashcode]++;
        }
      }
      Debug.Print($"nombre de clés : {stats.Keys.Count}");
    }
  }
}
