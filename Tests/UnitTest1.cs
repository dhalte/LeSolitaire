using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeSolitaireLogique;
using System.Diagnostics;
using System.Xml;
using System.Text;
using System.IO;

namespace Tests
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TestConversionsCoordonnees()
    {
      int xMin = -5, xMax = 3, yMin = -3, yMax = 7;
      TestConversionsCoordonnees(xMin, xMax, yMin, yMax);
      xMin = -3; xMax = 3; yMin = -3; yMax = 3;
      TestConversionsCoordonnees(xMin, xMax, yMin, yMax);
    }
    private void TestConversionsCoordonnees(int xMin, int xMax, int yMin, int yMax)
    {
      Etendue etendue = new Etendue(xMin, xMax, yMin, yMax);
      CoordonneesStock coordonneesStock = new CoordonneesStock(etendue);
      int nbCases = (xMax - xMin + 1) * (yMax - yMin + 1);
      for (int y = yMin; y <= yMax; y++)
      {
        for (int x = xMin; x <= xMax; x++)
        {
          Coordonnee c = coordonneesStock.GetCoordonnee(x, y);
          byte b = etendue.FromXY(c);
          Assert.IsTrue(0 <= b && b < nbCases);
          (int x, int y) p= etendue.FromByte(b);
          Coordonnee c1 = coordonneesStock.GetCoordonnee(p.x, p.y);
          Assert.IsTrue(c == c1);
        }
      }
    }

    public class Feedback : IFeedback
    {
      void IFeedback.Feedback(enumFeedbackHint hint, string msg)
      {
        Debug.Print($"{DateTime.Now:HH:mm:ss.fff} {hint} {msg}");
      }
    }
    [TestMethod]
    public void TestParLesDeuxBoutsConfig()
    {
      Feedback feedback = new Feedback();
      string configDescription = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
  <reprise idx=""999999999"" />
  <solution>
    <plateau>
  oxx
 xxxxx
xxxxxxx
xxxxxxx
xxxxxxx
 xxxxx
  xxx
    </plateau>
    <mouvement x=""1"" y=""-3"" dir=""o"" />
    <mouvement x=""0"" y=""-1"" dir=""n"" />
  </solution>
</solitaire>
";
      ParLesDeuxBoutsConfig config = new ParLesDeuxBoutsConfig(configDescription);
      XmlDocument xConfig = config.SauveConfig();
      string file = Path.GetTempFileName();

      xConfig.Save(file);
      Debug.Print($"{file}");
      string content = File.ReadAllText(file);
      Debug.Print($"{content}");     
    }
  }
}
