using BTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Tests
{
  [TestClass]
  public class TestBTreeVolatile
  {
    readonly int tailleElement = sizeof(uint);
    readonly int ordre = 32;
    readonly int nbElt = 1_000_000;
    readonly int maxElt = 20_000_000;
    int nbEltInseres = -1;
    readonly Cmp cmp;
    public TestBTreeVolatile()
    {
      cmp = new Cmp();
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
      Stopwatch sw = Stopwatch.StartNew();
      BTreeVolatile stock = new BTreeVolatile(tailleElement, ordre, cmp);
      stock.InitBTree();
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
      sw.Stop();
      Trace.WriteLine($"{nbEltInseres}/{nbElt} insérés en {sw.Elapsed}");
    }
    private unsafe void TestVerification()
    {
      BTreeVolatile stock = new BTreeVolatile(tailleElement, ordre, cmp);
      stock.InitBTree();
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

      Stopwatch sw = Stopwatch.StartNew();
      int elementPrecedent = int.MinValue;
      int nbElementsLus = 0;
      foreach (var element in stock.EnumereElements())
      {
        int elementLu = ConvertByte2Int(element);
        Assert.IsTrue(elementPrecedent < elementLu);
        nbElementsLus++;
      }
      Assert.IsTrue(nbElementsLus == nbEltInseres);
      sw.Stop();
      Trace.WriteLine($"Relecture effectuée en {sw.Elapsed}");
    }

    private unsafe void CollecteStats()
    {
      BTreeVolatile stock = new BTreeVolatile(tailleElement, ordre, cmp);
      stock.InitBTree();
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
      Stopwatch sw = Stopwatch.StartNew();
      List<StatsProfondeur> stats = stock.CollecteStats();
      sw.Stop();
      Trace.WriteLine($"Collecte stats effectuée en {sw.Elapsed}");
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("profondeur;nb noeuds;nb éléments");
      for (int idxProfondeur = stats.Count - 1; idxProfondeur >= 0; idxProfondeur--)
      {
        StatsProfondeur s = stats[idxProfondeur];
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
    private class Cmp : IComparateurSituations
    {
      public unsafe int CompareSituations(byte* p1, byte* p2)
      {
        return TestBTreeVolatile.ConvertByte2Int(p1).CompareTo(TestBTreeVolatile.ConvertByte2Int(p2));
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

    [TestMethod]
    public unsafe void TestEnumeration()
    {
      int ordre = 4;
      int nbElt = 100;
      int maxElt = 2_000;
      int nbEltInseres = -1;
      List<int> listElements = new List<int>();
      Stopwatch sw = Stopwatch.StartNew();
      BTreeVolatile stock = new BTreeVolatile(tailleElement, ordre, cmp);
      stock.InitBTree();
      Random rnd = new Random(0);
      byte[] data = new byte[tailleElement];
      nbEltInseres = 0;
      int idxElt;
      for (idxElt = 0; idxElt < nbElt; idxElt++)
      {
        int elt = rnd.Next(maxElt);
        ConvertInt2Byte(elt, data);
        fixed (byte* pData = data)
        {
          InsertOrUpdateResult result = stock.InsertOrUpdate(pData);
          //Debug.Print($"{elt}, {result}");
          if (result == InsertOrUpdateResult.Inserted)
          {
            listElements.Add(elt);
            nbEltInseres++;
          }
        }
      }
      sw.Stop();
      Trace.WriteLine($"{nbEltInseres}/{nbElt} insérés en {sw.Elapsed}");
      listElements.Sort();
      idxElt = 0;
      // Enumération complète
      foreach (byte[] data1 in stock.EnumereElements())
      {
        int elt = ConvertByte2Int(data1);
        Assert.IsTrue(elt == listElements[idxElt++]);
      }
      Assert.IsTrue(idxElt == listElements.Count);
      // Enumération à partir de chaque élément de la liste
      for (int idxEltStart = 0; idxEltStart < listElements.Count; idxEltStart++)
      {
        ConvertInt2Byte(listElements[idxEltStart], data);
        // Enumération à partir d'un élément donné de la liste
        idxElt = idxEltStart;
        foreach (byte[] data1 in stock.EnumereElements(data))
        {
          int elt = ConvertByte2Int(data1);
          Assert.IsTrue(elt == listElements[idxElt++]);
        }
        Assert.IsTrue(idxElt == listElements.Count);
      }
    }

    [TestMethod]
    public unsafe void TestExistence()
    {
      int ordre = 4;
      int nbElt = 100;
      int maxElt = 2_000;
      int nbEltInseres = -1;
      List<int> listElements = new List<int>();
      Stopwatch sw = Stopwatch.StartNew();
      BTreeVolatile stock = new BTreeVolatile(tailleElement, ordre, cmp);
      stock.InitBTree();
      Random rnd = new Random(0);
      byte[] data = new byte[tailleElement];
      nbEltInseres = 0;
      int idxElt;
      for (idxElt = 0; idxElt < nbElt; idxElt++)
      {
        int elt = rnd.Next(maxElt);
        ConvertInt2Byte(elt, data);
        fixed (byte* pData = data)
        {
          InsertOrUpdateResult result = stock.InsertOrUpdate(pData);
          //Debug.Print($"{elt}, {result}");
          if (result == InsertOrUpdateResult.Inserted)
          {
            listElements.Add(elt);
            nbEltInseres++;
          }
        }
      }
      sw.Stop();
      Trace.WriteLine($"{nbEltInseres}/{nbElt} insérés en {sw.Elapsed}");
      listElements.Sort();
      idxElt = 0;
      for (int elt = -5; elt < maxElt + 5; elt++)
      {
        while (idxElt < listElements.Count && listElements[idxElt] < elt)
        {
          idxElt++;
        }
        bool bExistAttendu = (idxElt < listElements.Count && listElements[idxElt] == elt);
        ConvertInt2Byte(elt, data);
        fixed (byte* pSituation = data)
        {
          bool bExist = stock.Existe(pSituation);
          Assert.IsTrue(bExistAttendu == bExist);
        }
      }
    }

  }
}
