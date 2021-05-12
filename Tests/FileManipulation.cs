using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Tests
{
  /// <summary>
  /// Description résumée pour FileManipulation
  /// </summary>
  [TestClass]
  public class FileManipulation
  {

    [TestMethod]
    public void TestReadWrite()
    {
      const string nomFichier = "tmp.dat";
      FileInfo fi = new FileInfo(nomFichier);
      Debug.Print(fi.FullName);
      if (fi.Exists)
      {
        fi.Delete();
      }
      const int len = 10;
      byte[] buffer = new byte[len];
      UInt32[] uibuffer = new UInt32[len];
      UInt32 ui = UInt32.MaxValue;
      for (int idx = 0; idx < len; idx++)
      {
        buffer[idx] = (byte)('a' + idx);
        uibuffer[idx] = ui--;
      }
      using (FileStream fs = new FileStream(fi.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
      {
        // écrire abcdefghij
        fs.Write(buffer, 0, len);
        // Utiliser using (BinaryWriter bw = new BinaryWriter(fs)) ferme le fichier à la fin du using, et donc le fs.write suivant plante
        BinaryWriter bw = new BinaryWriter(fs);
        // écrire FFFF FFFE FFFD FFFC FFFB FFFA FFF9 FFF8 FFF7 FFF6
        for (int idx = 0; idx < len; idx++)
        {
          bw.Write(uibuffer[idx]);
        }
        Array.Reverse(uibuffer);
        // écrire FFF6 FFF7 FFF8 FFF9 FFFA FFFB FFFC FFFD FFFE FFFF
        for (int idx = 0; idx < len; idx++)
        {
          bw.Write(uibuffer[idx]);
        }
        Array.Reverse(buffer);
        // écrire jihgfedcba
        fs.Write(buffer, 0, len);
      }
      using (FileStream fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        // lire abcdefghij
        Assert.AreEqual(fs.Read(buffer, 0, len), len);
        string s = "";
        for (int idx = 0; idx < len; idx++)
        {
          s += (char)(buffer[idx]);
        }
        BinaryReader br = new BinaryReader(fs);
        // lire FFFFFFFF FFFFFFFE FFFFFFFD FFFFFFFC FFFFFFFB FFFFFFFA FFFFFFF9 FFFFFFF8 FFFFFFF7 FFFFFFF6
        for (int idx = 0; idx < len; idx++)
        {
          uibuffer[idx] = br.ReadUInt32();
        }
        for (int idx = 0; idx < len; idx++)
        {
          s = $"{s}\t{uibuffer[idx]:X8}";
        }
        // lire FFFFFFF6 FFFFFFF7 FFFFFFF8 FFFFFFF9 FFFFFFFA FFFFFFFB FFFFFFFC FFFFFFFD FFFFFFFE FFFFFFFF
        for (int idx = 0; idx < len; idx++)
        {
          uibuffer[idx] = br.ReadUInt32();
        }
        for (int idx = 0; idx < len; idx++)
        {
          s = $"{s}\t{uibuffer[idx]:X4}";
        }
        Assert.AreEqual(fs.Read(buffer, 0, len), len);
        s += '\t';
        for (int idx = 0; idx < len; idx++)
        {
          s += (char)(buffer[idx]);
        }
        // lire jihgfedcba
        Assert.AreEqual(fs.Position, fs.Length);
        Debug.Print(s);
      }
    }
  }
}
