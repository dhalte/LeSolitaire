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
      Config parLesDeuxBoutsConfig = new Config(config);
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
      Config parLesDeuxBoutsConfig = new Config(config);
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

  }
}
