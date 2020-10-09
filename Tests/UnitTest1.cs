using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeSolitaireLogique;
using System.Diagnostics;
using System.Xml;
using System.Text;
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
    // Test du mécanisme de comparaison d'une Situation avec la SituationEtude correspondante
    [TestMethod]
    public void TestComparaisonSituationSituationEtendue()
    {
      string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<solitaire>
  <parametres nd=""9"" nf=""12"" />
  <plateau>
  ooo
 ooxoo
ooooooo
oooxxoo
ooooooo
 ooooo
  ooo
  </plateau>
</solitaire>";
      Pilote parLesDeuxBoutsConfig = new Pilote(config);
      Plateau plateau = new Plateau(parLesDeuxBoutsConfig.PlateauRaw);
      SituationRaw plateauRaw = parLesDeuxBoutsConfig.PlateauRaw;
      Situation situation = new Situation(plateau.Etendue, plateauRaw);
      SituationEtude situationEtude = new SituationEtude(plateau);
      situationEtude.ChargeSituationRaw(plateauRaw);
      // Juste pour le test
      Array.Copy(situationEtude.Pierres, situationEtude.ImagePierres, plateau.Etendue.NbCasesRectangleEnglobant);
      HashSet<SituationBase> situations = new HashSet<SituationBase>();
      situations.Add(situation);
      bool b = situations.Contains(situationEtude);
      Assert.IsTrue(b);
    }

    [TestMethod]
    public void HexDump()
    {
      string filename = @"C:\Users\halte\reposDivers\LeSolitaire\Jeux\Plateau Français\ED.dat";
      long sz = 0;
      byte[] buf = null;
      using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
      {
        sz = fs.Length;
        buf = new byte[sz];
        fs.Read(buf, 0, (int)sz);
      }
      StringBuilder sb = new StringBuilder();
      for (int idx = 0; idx < sz; idx++)
      {
        if (idx % 8 == 0)
        {
          sb.AppendLine();
        }
        else if (idx % 4 == 0)
        {
          sb.Append("  ");
        }
        sb.Append($" {buf[idx]:x2}");
      }
      Debug.Print(sb.ToString());
    }

    [TestMethod]
    public void TestModifCollection()
    {
      List<SituationInitiale> EI = new List<SituationInitiale>();
      bool[] pierres = new bool[37];
      for (int i = 0; i < 15; i++)
      {
        Situation s = new Situation(pierres);
        SituationInitiale si = new SituationInitiale(s);
        if (i % 2 == 1) si.Resolue = true;
        EI.Add(si);
      }
      foreach (SituationInitiale si in EI.FindAll(si => !si.Resolue))
      {
        si.Resolue = true;
      }
    }


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
    [TestMethod]
    public void EvaluationVarianceSituationsGagnantes()
    {
      string descriptionPlateau = @"
  xxx
 xxxxx
xxxxxxx
xxxxxxx
xxxxxxx
 xxxxx
  xxx
";
      string fileName = @"C:\Users\halte\reposDivers\LeSolitaire\Jeux\Plateau Français\EF.dat";
      // il y a 1 pierre dans une SG, 13 mouvements qui séparent une SF d'une SG, donc 14 pierres dans une SF.
      int NbPierres = 14;
      // Le plateau est constitué de 7 colonnes
      int NbColonnes = 7;
      byte[] buffer = new byte[NbPierres];
      Plateau plateau = new Plateau(Common.ChargeSituationRaw(descriptionPlateau));
      using (FileStream EFdat = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      {
        long EFlen = EFdat.Length;
        Assert.IsTrue(EFlen % NbPierres == 0);
        // Une première étude a montré que les variances étaient comprises entre 0 et 8
        // Je constitue une statistique de ces variances en les comptant par tranches de 0.01, ce qui fait au plus 800 tranches
        int[] statistiques = new int[800];
        long idxSituation = 0;
        float Gt = 0, Vt = 0, Vmax = 0;
        for (; ; )
        {
          int n = EFdat.Read(buffer, 0, NbPierres);
          if (n == 0)
          {
            break;
          }
          Assert.IsTrue(n == NbPierres);
          // Calcul de la variance de cette situation
          long X2 = 0, Y2 = 0, X = 0, Y = 0;
          for (int idxPierreInBuffer = 0; idxPierreInBuffer < NbPierres; idxPierreInBuffer++)
          {
            int idxPierre = buffer[idxPierreInBuffer];
            int x = idxPierre % NbColonnes, y = idxPierre / NbColonnes;
            X += x;
            Y += y;
            X2 += x * x;
            Y2 += y * y;
          }
          float V = 1f / NbPierres * (X2 + Y2 - 1f / NbPierres * (X * X + Y * Y));
          Assert.IsTrue(V > 0);
          if (Vmax < V)
          {
            Debug.Print($"idxSituation={idxSituation}, V={V}");
            Situation situation = new Situation(buffer);
            Debug.Print($"{situation.Dump(plateau)}");
            Vmax = V;
          }
          int idxStatistique = (int)(100 * V);
          statistiques[idxStatistique]++;
          idxSituation++;
          Gt += V;
          Vt += V * V;
        }
        Debug.Print($"Gt={Gt / idxSituation}, Vt={1f / idxSituation * (Vt - 1f / idxSituation * Gt * Gt)}");
        for (int idxStatique = 0; idxStatique < statistiques.Length; idxStatique++)
        {
          Debug.Print($"{idxStatique / 100f:0.000} {statistiques[idxStatique]}");
        }
      }
    }

    // Evaluation des collisions dans la table EF.dat
    // Voir LeSolitaireLogique.Tests.EvaluePertinenceHashage()
    // J'ai essayé d'exécuter le test ici.
    // Le problème est que le test s'exécute dans un environnement qui provoque des OutOfMemory exceptions
    // Même en ajoutant un App.config avec
    //    <runtime>     <gcAllowVeryLargeObjects enabled = "true" />   </ runtime >
    // cela n'a rien arrangé.
    // Alors j'ai fait exécuter le test au démarrage ( Program.Main() ).

    [TestMethod]
    public void TestUINT()
    {
      uint a = (uint)int.MaxValue;
      long b = a;
      Debug.Print($"a={a}, {a:x}, b={b}, {b:x}");
      a = uint.MaxValue;
      b = a;
      Debug.Print($"a={a}, {a:x}, b={b}, {b:x}");
    }

    [TestMethod]
    public void TestSortedSet()
    {
      SortedSet<int> ss = new SortedSet<int>();
      Random random = new Random();
      for (int i = 0; i < 100_000; i++)
      {
        int n = random.Next(0, 5_000_000);
        if (!ss.Contains(n))
        {
          ss.Add(n);
        }
      }
      int p = int.MinValue;
      foreach (int n in ss)
      {
        Assert.IsTrue(p < n);
        p = n;
      }
    }
  }
}
