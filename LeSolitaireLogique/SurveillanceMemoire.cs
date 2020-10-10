using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class SurveillanceMemoire
  {
    enum MemoryResourceNotificationType : int
    {
      LowMemoryResourceNotification = 0,
      HighMemoryResourceNotification = 1,
    }
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateMemoryResourceNotification(MemoryResourceNotificationType notificationType);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool QueryMemoryResourceNotification(IntPtr resourceNotificationHandle, out int resourceState);

    private IntPtr MemoryResourceNotificationHandle;

    public event EventHandler LowMemoryEvent;
    public void Start()
    {
      Task task = new Task(Surveille);
      task.Start();
    }

    private void Surveille()
    {
      MemoryResourceNotificationHandle = CreateMemoryResourceNotification(MemoryResourceNotificationType.LowMemoryResourceNotification);

      int sleepIntervalInMs = 5 * 1000;

      while (true)
      {
        Thread.Sleep(sleepIntervalInMs);
        bool isSuccecced = QueryMemoryResourceNotification(MemoryResourceNotificationHandle, out int memoryStatus);
        if (isSuccecced)
        {
          if (memoryStatus >= 1)
          {
            Debug.Print($"Surveille isSuccecced {isSuccecced} memoryStatus {memoryStatus}");
            LowMemoryEvent?.Invoke(this, EventArgs.Empty);
          }
        }
      }
    }
  }
}
