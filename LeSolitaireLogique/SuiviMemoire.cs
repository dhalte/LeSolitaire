using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class SuiviMemoire
  {
    private string FichierLog;
    private readonly Timer Timer;
    private const string Entete = "\ntime\tNonpagedSystemMemorySize64\tPagedMemorySize64\tPagedSystemMemorySize64\tPeakPagedMemorySize64\tPeakVirtualMemorySize64\tPeakWorkingSet64\tPrivateMemorySize64\tVirtualMemorySize64\tWorkingSet64\n";
    public SuiviMemoire(string fichierLog, TimeSpan periode)
    {
      FichierLog = fichierLog;
      File.AppendAllText(FichierLog, Entete);
      Timer = new Timer(callbackproc, null, periode, periode);
    }

    private void callbackproc(object state)
    {
      Process p = Process.GetCurrentProcess();
      File.AppendAllText(FichierLog, $"{DateTime.Now:HH:mm:ss}\t{p.NonpagedSystemMemorySize64}\t{p.PagedMemorySize64}\t{p.PagedSystemMemorySize64}\t{p.PeakPagedMemorySize64}\t{p.PeakVirtualMemorySize64}\t{p.PeakWorkingSet64}\t{p.PrivateMemorySize64}\t{p.VirtualMemorySize64}\t{p.WorkingSet64}\n");
    }

  }
}
