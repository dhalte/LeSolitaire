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
  public class TestBTreePersistant
  {
    readonly DirectoryInfo diRepertoire;
    readonly int tailleElement = sizeof(uint);
    readonly int ordre = 32;
    readonly int nbElt = 1_000_000;
    readonly int maxElt = 20_000_000;
    int nbEltInseres = -1;
    readonly Cmp cmp;
    public TestBTreePersistant()
    {
      string repertoire = "tmp";
      diRepertoire = new DirectoryInfo(repertoire);
      repertoire = diRepertoire.FullName;
      Debug.Print(repertoire);
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
      if (diRepertoire.Exists)
      {
        diRepertoire.Delete(true);
      }
      diRepertoire.Create();
      Stopwatch sw = Stopwatch.StartNew();
      BTreePersistant stock = new BTreePersistant(diRepertoire, tailleElement, ordre, cmp);
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
      stock.Flush();
      stock.Close();
      sw.Stop();
      Trace.WriteLine($"{nbEltInseres}/{nbElt} insérés en {sw.Elapsed}");
    }
    private void TestVerification()
    {
      Stopwatch sw = Stopwatch.StartNew();
      BTreePersistant stock = new BTreePersistant(diRepertoire, tailleElement, ordre, cmp);
      stock.InitBTree();
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
      BTreePersistant stock = new BTreePersistant(diRepertoire, tailleElement, ordre, cmp);
      stock.InitBTree();
      List<StatsProfondeur> stats = stock.CollecteStats();
      stock.Close();
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
        return TestBTreePersistant.ConvertByte2Int(p1).CompareTo(TestBTreePersistant.ConvertByte2Int(p2));
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
      if (diRepertoire.Exists)
      {
        diRepertoire.Delete(true);
      }
      diRepertoire.Create();
      Stopwatch sw = Stopwatch.StartNew();
      BTreePersistant stock = new BTreePersistant(diRepertoire, tailleElement, ordre, cmp);
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
      stock.Flush();
      stock.Close();
      sw.Stop();
      Trace.WriteLine($"{nbEltInseres}/{nbElt} insérés en {sw.Elapsed}");
      stock = new BTreePersistant(diRepertoire, tailleElement, ordre, cmp);
      stock.InitBTree();
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
      stock.Close();
    }

    [TestMethod]
    public unsafe void TestExistence()
    {
      int ordre = 4;
      int nbElt = 100;
      int maxElt = 2_000;
      int nbEltInseres = -1;
      List<int> listElements = new List<int>();
      if (diRepertoire.Exists)
      {
        diRepertoire.Delete(true);
      }
      diRepertoire.Create();
      Stopwatch sw = Stopwatch.StartNew();
      BTreePersistant stock = new BTreePersistant(diRepertoire, tailleElement, ordre, cmp);
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
      stock.Flush();
      stock.Close();
      sw.Stop();
      Trace.WriteLine($"{nbEltInseres}/{nbElt} insérés en {sw.Elapsed}");
      stock = new BTreePersistant(diRepertoire, tailleElement, ordre, cmp);
      stock.InitBTree();
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
      stock.Close();
    }

    [TestMethod]
    public unsafe void TestLibereMemoire()
    {
      if (diRepertoire.Exists)
      {
        diRepertoire.Delete(true);
      }
      diRepertoire.Create();
      BTreePersistant stock = new BTreePersistant(diRepertoire, tailleElement, ordre, cmp);
      stock.InitBTree();
      Random rnd = new Random(0);
      byte[] data = new byte[tailleElement];
      nbEltInseres = 0;
      Stopwatch sw = Stopwatch.StartNew();
      for (int idxTest = 0; idxTest < 5; idxTest++)
      {
        Stopwatch sw1 = Stopwatch.StartNew();
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
        Trace.WriteLine($"durée insertions : {sw1.ElapsedMilliseconds} ms");
        sw1.Restart();
        stock.Flush();
        Trace.WriteLine($"durée flush : {sw1.ElapsedMilliseconds} ms");
        sw1.Restart();
        long beforeCollecte = GC.GetTotalMemory(false);
        stock.LibereMemoire();
        GC.Collect();
        long afterCollecte = GC.GetTotalMemory(false);
        Trace.WriteLine($"GC before {beforeCollecte}, after {afterCollecte}, durée collecte : {sw1.ElapsedMilliseconds} ms");
      }
      Trace.WriteLine($"nb inserés : {nbEltInseres}");
      int elementPrecedent = int.MinValue;
      int nbElementsLus = 0;
      sw.Restart();
      foreach (var element in stock.EnumereElements())
      {
        int elementLu = ConvertByte2Int(element);
        Assert.IsTrue(elementPrecedent < elementLu);
        nbElementsLus++;
      }
      Assert.IsTrue(nbElementsLus == nbEltInseres);
      Trace.WriteLine($"durée vérification : {sw.ElapsedMilliseconds} ms");
      sw.Restart();
      stock.Close();
      Trace.WriteLine($"durée close : {sw.ElapsedMilliseconds} ms");
    }
  }
}
