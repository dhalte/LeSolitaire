using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace Tests
{
  [TestClass]
  public class ByteManipulation
  {
    // Un byte est composé de 8 bits. Ci dessous, une représentation de ces bits, poids forts à gauche
    // +--- 7 ---+--- 6 ---+--- 5 ---+--- 4 ---+--- 3 ---+--- 2 ---+--- 1 ---+--- 0 ---+
    // La valeur de chaque bit (exprimée en hexadécimal)
    // +-- 80 ---+-- 40 ---+-- 20 ---+-- 10 ---+--- 8 ---+--- 4 ---+--- 2 ---+--- 1 ---+
    // La somme de ces valeurs fait 255
    // La valeur complémentaire de chaque bit (exprimée en hexadécimal)
    // +-- 7F ---+-- BF ---+-- DF ---+-- EF ---+-- F7 ---+-- FB ---+-- FD ---+-- FE ---+
    [TestMethod]
    public void TestMethod1()
    {
      byte[] P = new byte[5];
      P[3] |= 8;
      P[4] |= 1 << 3;
      Assert.AreEqual(P[3], P[4]);
      bool b;
      b = (P[3] & 8) != 0;
      Assert.IsTrue(b);
      b = (P[3] & 2) == 0;
      Assert.IsTrue(b);
      P[3] &= 0xF7;
      b = P[3] == 0;
      Assert.IsTrue(b);
    }
    [TestMethod]
    public void TestByte()
    {
      byte b1, b2;
      b1 = 0xfe;
      b2 = (byte)~b1;
      Debug.Print($"b1={Convert.ToString(b1, 2),8}");
      Debug.Print($"b2={Convert.ToString(b2, 2),8}");
    }
    [TestMethod]
    public void TestShift()
    {
      byte b1 = (byte)(1 << 7);
      Debug.Print($"{b1:X2} {Convert.ToString(b1, 2),8}");
      int shift = 8;
      // Ne provoque pas d'exception
      b1 = (byte)(1 << shift);
      Debug.Print($"{b1:X2} {Convert.ToString(b1, 2),8}");
    }
  }
}
