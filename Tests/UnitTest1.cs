using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeSolitaireLogique;
using System.Diagnostics;
using System.Xml;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Tests
{
  [TestClass]
  public class UnitTest1
  {
    // Test du chargement d'un fichier pilote
    [TestMethod]
    public void TestInitPlateau()
    {
      string config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<solitaire>
  <parametres nd=""9"" nf=""12"" />
  <plateau>
  xxx
 xxxxx
xxxxxxx
xxxxxxx
xxxxxxx
 xxxxx
  xxx
  </plateau>
</solitaire>";
      Pilote parLesDeuxBoutsConfig = new Pilote(config);
      Plateau plateau = new Plateau(parLesDeuxBoutsConfig.PlateauRaw);
    }

    // Test du mécanisme de comparaison d'une Situation avec la SituationEtude correspondante
    [TestMethod]
    public void TestSituationSituationEtendue()
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


    public class A
    {
      public int a;
      public override bool Equals(object obj)
      {
        return a == ((A)obj).a;
      }
      public override int GetHashCode()
      {
        return a;
      }
    }
    public class B : A { }

    [TestMethod]
    public void TestCast()
    {
      HashSet<A> list = new HashSet<A>();
      B B = new B { a = 1 };
      list.Add(B);
      B B1 = new B { a = 1 };
      A A2 = null;
      B B2 = null;
      bool exist = list.TryGetValue(B1, out A2);
      if (exist)
      {
        B2 = (B)A2;
      }

    }
  }
}
