using LeSolitaireStockage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Tests
{
  [TestClass]
  public class TestBTreeStockage
  {
    DirectoryInfo diRepertoire;
    int tailleElement = sizeof(uint);
    int ordre = 32;
    int nbElt = 1_000_000;
    int maxElt = 20_000_000;
    int nbEltInseres = -1;
    Cmp cmp;
    public TestBTreeStockage()
    {
      string repertoire = "tmp";
      diRepertoire = new DirectoryInfo(repertoire);
      repertoire = diRepertoire.FullName;
      Debug.Print(repertoire);
      cmp = new Cmp(tailleElement);
    }

    // 975 388/1 000 000 insérés en ≈ 14 secondes
    // Relecture effectuée en ≈ 0.6 secondes
    [TestMethod]
    public void TestInsertionVerification()
    {
      TestInsertion();
      TestVerification();
      CollecteStats();
    }

    private unsafe void TestInsertion()
    {
      if (diRepertoire.Exists)
      {
        diRepertoire.Delete(true);
      }
      diRepertoire.Create();
      Stopwatch sw = Stopwatch.StartNew();
      Stockage stock = new Stockage(diRepertoire, tailleElement, ordre, cmp);
      Random rnd = new Random(0);
      byte[] data = new byte[tailleElement];
      nbEltInseres = 0;
      for (int idxElt = 0; idxElt < nbElt; idxElt++)
      {
        int elt = rnd.Next(maxElt);
        ConvertInt2Byte(elt, data);
        fixed (byte* pData = data)
        {
          InsertOrUpdateResult result = stock.InsertOrUpdate(pData);
          //Debug.Print($"{elt}, {result}");
          if (result == InsertOrUpdateResult.Inserted)
          {
            nbEltInseres++;
          }
        }
      }
      stock.Flush();
      stock.Close();
      sw.Stop();
      Trace.WriteLine($"{nbEltInseres}/{nbElt} insérés en {sw.Elapsed}");
    }
    private void TestVerification()
    {
      Stopwatch sw = Stopwatch.StartNew();
      Stockage stock = new Stockage(diRepertoire, tailleElement, ordre, cmp);
      int elementPrecedent = int.MinValue;
      int nbElementsLus = 0;
      foreach (var element in stock.EnumereElements())
      {
        int elementLu = ConvertByte2Int(element);
        Assert.IsTrue(elementPrecedent < elementLu);
        nbElementsLus++;
      }
      Assert.IsTrue(nbElementsLus == nbEltInseres);
      stock.Close();
      sw.Stop();
      Trace.WriteLine($"Relecture effectuée en {sw.Elapsed}");
    }
    
    private void CollecteStats()
    {
      Stopwatch sw = Stopwatch.StartNew();
      Stockage stock = new Stockage(diRepertoire, tailleElement, ordre, cmp);
      List<Stockage.StatsProfondeur> stats = stock.CollecteStats();
      stock.Close();
      sw.Stop();
      Trace.WriteLine($"Collecte stats effectuée en {sw.Elapsed}");
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("profondeur;nb noeuds;nb éléments");
      for (int idxProfondeur = stats.Count-1; idxProfondeur >= 0; idxProfondeur--)
      {
        Stockage.StatsProfondeur s = stats[idxProfondeur];
        sb.AppendLine($"{idxProfondeur};{s.nbNoeuds};{s.nbElements}");
      }
      Trace.WriteLine(sb.ToString());
    }
    private static void ConvertInt2Byte(int n, byte[] b)
    {
      b[3] = (byte)(n >> 24);
      b[2] = (byte)((n >> 16) & 0xff);
      b[1] = (byte)((n >> 8) & 0xff);
      b[0] = (byte)(n & 0xff);
    }
    private static int ConvertByte2Int(byte[] b)
    {
      return (b[3] << 24) | (b[2] << 16) | (b[1] << 8) | b[0];
    }
    private unsafe static int ConvertByte2Int(byte* b)
    {
      return (b[3] << 24) | (b[2] << 16) | (b[1] << 8) | b[0];
    }
    private class Cmp : ComparateurSituations
    {
      private int TailleElement;
      public Cmp(int tailleElement)
      {
        TailleElement = tailleElement;
      }
      public unsafe int CompareSituations(byte* p1, byte* p2)
      {
        return TestBTreeStockage.ConvertByte2Int(p1).CompareTo(TestBTreeStockage.ConvertByte2Int(p2));
      }

      public unsafe bool MajSituation(byte* pSituationNew, byte* pSituationExistante)
      {
        return false;
      }

      public unsafe string ToString(byte* pElement)
      {
        return ConvertByte2Int(pElement).ToString();
      }

      public string ToString(byte[] pElement)
      {
        return ConvertByte2Int(pElement).ToString();
      }

    }

  }
}
