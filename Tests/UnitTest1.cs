using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace Tests
{
  [TestClass]
  public class UnitTest1
  {


    // A quoi sert l'attribut [Flags] d'une énumération
    // Semble-t-il uniquement à influencer le ToString() d'une valeur de cette énumération
    // i;A;B
    // 0;a;a
    // 1;b;b
    // 2;c;c
    // 3;3;b, c
    // 4;d;d
    // 5;5;b, d
    // 6;cd;cd
    // 7;7;b, cd
    // 8;e;e
    // 9;9;b, e
    // 10;10;c, e
    // 11;11;b, c, e
    // 12;12;d, e
    // 13;13;b, d, e
    // 14;14;cd, e
    // 15;15;b, cd, e
    // 16;f;f
    // 17;17;b, f
    // 18;18;c, f
    // 19;19;b, c, f
    // 20;20;d, f
    // 21;21;b, d, f
    // 22;22;cd, f
    // 23;23;b, cd, f
    // 24;24;e, f
    // 25;25;b, e, f
    // 26;26;c, e, f
    // 27;27;b, c, e, f
    // 28;28;d, e, f
    // 29;29;b, d, e, f
    // 30;30;cd, e, f
    // 31;31;b, cd, e, f
    // 32;32;32
    // 33;33;33
    // On remarque toutefois que si l'énumération contient un membre agrégé,
    // comme ici cd = c | d = 6, alors c'est l'agrégé qui est retenu et les flags individuels sont exclus.
    //  7 = b | c | d = b | cd, ToString() affiche b, cd. J'imaginais plutôt que ce serait b, c, d, cd.
    public enum enumA
    {
      a = 0,
      b = 1,
      c = 2,
      d = 4,
      cd = 6,
      e = 8,
      f = 16
    }

    [Flags]
    public enum enumB
    {
      //                __111 =  7
      //                _1_1_ = 10
      a = 0,
      b = 1,         // ____1
      c = 2,         // ___1_
      d = 4,         // __1__
      cd = 6,        // __11_
      e = 8,         // _1___
      f = 16         // 1____
    }

    [TestMethod]
    public void TesteEnum()
    {
      Debug.Print($"i;A;B");
      for (int i = 0; i < 34; i++)
      {
        enumA A = (enumA)i;
        enumB B = (enumB)i;
        Debug.Print($"{i};{A};{B}");
      }
    }

    // Evaluation de la variance des situations finales

    // Où on vérifie que la version Array.Sort(keys,arr) tri aussi les keys
    [TestMethod]
    public void TestTriArray()
    {
      int l = 5;
      string[] s = new string[l];
      int[] k = new int[l];
      Random rnd = new Random();
      for (int i = 0; i < l; i++)
      {
        k[i] = rnd.Next(0, 5 * l);
        string n = "";
        for (int j = 0; j < 5; j++)
        {
          n += (char)('a' + rnd.Next(0, 25));
        }
        s[i] = n;
      }
      Debug.Print("Avant tri");
      string t = "";
      for (int i = 0; i < l; i++)
      {
        t = $"{t}{k[i]} {s[i]}\t";
      }
      Debug.Print(t);
      Array.Sort(k, s);
      Debug.Print("Après tri");
      t = "";
      for (int i = 0; i < l; i++)
      {
        t = $"{t}{k[i]} {s[i]}\t";
      }
      Debug.Print(t);
    }

    // La simple déclaration d'un uplet suffit à lui allouer de la mémoire
    // on peut aussi l'utiliser pour lui assigner de nouvelles valeurs, je ne sais pas si cela se fait
    // par réutilisation de la mémoire déjà allouée ou allocation d'une nouvelle zone mémoire.
    // On ne peut utiliser fixed() sur des uplets, mais on peut le faire sur ses membres.
    (byte[] b, int i) uplet;

    [TestMethod]
    public unsafe void TestUplet()
    {
      uplet.b = new byte[2];
      uplet.b[0] = 5;
      uplet.b[1] = 10;
      uplet.i = 2;
      Debug.Print($"{uplet.b[0]}, {uplet.b[1]}, {uplet.i}");

      uplet = (new byte[5], 12);
      uplet.b[0] = 15;
      uplet.b[1] = 100;
      uplet.i = 12;
      Debug.Print($"{uplet.b[0]}, {uplet.b[1]}, {uplet.i}");

      fixed (byte* p = uplet.b)
      {
        Debug.Print($"@p = {new IntPtr(p)}");
      }
    }

    // On vérifie que Array.Copy(T,T,) fonctionne
    [TestMethod]
    public unsafe void TestCopyArray()
    {
      int[] a1 = new int[5];
      for (int i = 0; i < 5; i++)
      {
        a1[i] = i;
      }
      Array.Copy(a1, a1, 5);
      for (int i = 0; i < 5; i++)
      {
        Assert.IsTrue(a1[i] == i);
      }
    }

  }
}
