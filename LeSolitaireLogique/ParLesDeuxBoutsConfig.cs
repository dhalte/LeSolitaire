using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LeSolitaireLogique
{
  public class ParLesDeuxBoutsConfig
  {
    public int Nd { get; private set; }
    public int Nf { get; private set; }
    private long IdxReprise;
    // C'est cet objet qui contient l'étendue du plateau
    public SituationRaw Plateau;
    private List<Solution> Solutions;
    public ParLesDeuxBoutsConfig(FileInfo file)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.Load(file.FullName);
      ChargeConfig(xmlDocument);
    }
    public ParLesDeuxBoutsConfig(string contenu)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(contenu);
      ChargeConfig(xmlDocument);
    }

    private void ChargeConfig(XmlDocument xmlDocument)
    {
      XmlElement xRoot = xmlDocument.DocumentElement;
      XmlElement xParam = xRoot["parametres"];
      Nd = int.Parse(xParam.GetAttribute("nd"));
      Nf = int.Parse(xParam.GetAttribute("nf"));
      XmlElement xPlateau = xRoot["plateau"];
      string plateau = xPlateau.InnerText;
      Plateau = ChargePlateau(plateau);
      XmlElement xReprise = xRoot["reprise"];
      IdxReprise = xReprise == null ? 0 : long.Parse(xReprise.GetAttribute("idx"));
      Solutions = new List<Solution>();
      foreach (XmlElement xSolution in xRoot.SelectNodes("solution"))
      {
        Solutions.Add(DecodeSolution(xSolution));
      }

    }

    private Solution DecodeSolution(XmlElement xSolution)
    {
      XmlElement xSituation = xSolution["plateau"];
      Solution solution = new Solution();
      solution.SituationInitiale = ChargePlateau(xSituation.InnerText);
      solution.Mouvements = new List<SolutionMouvement>();
      foreach (XmlElement xMouvement in xSolution.SelectNodes("mouvement"))
      {
        int x = int.Parse(xMouvement.GetAttribute("x"));
        int y = int.Parse(xMouvement.GetAttribute("y"));
        // On s'affranchit du stock de coordonnées
        Coordonnee pierre = new Coordonnee(x, y);
        enumDirection direction = Common.DecodeDirection(xMouvement.GetAttribute("dir"));
        SolutionMouvement mouvement = new SolutionMouvement(pierre, direction);
        solution.Mouvements.Add(mouvement);
      }
      return solution;
    }

    private SituationRaw ChargePlateau(string description)
    {
      SituationRaw plateau = Common.ChargeContenuStringSituation(description);
      plateau.CentrePoints();
      return plateau;
    }

    public XmlDocument SauveConfig()
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null));
      XmlElement xRoot = xmlDocument.CreateElement("solitaire");
      xmlDocument.AppendChild(xRoot);
      XmlElement xParametres = xmlDocument.CreateElement("parametres");
      xRoot.AppendChild(xParametres);
      xParametres.SetAttribute("nd", Nd.ToString());
      xParametres.SetAttribute("nf", Nf.ToString());
      XmlElement xPlateau = xmlDocument.CreateElement("plateau");
      xRoot.AppendChild(xPlateau);
      xPlateau.InnerText = SauvePlateau(Plateau);
      XmlElement xReprise = xmlDocument.CreateElement("reprise");
      xRoot.AppendChild(xReprise);
      xReprise.SetAttribute("idx", IdxReprise.ToString());
      foreach (Solution solution in Solutions)
      {
        SauveSolution(xRoot, solution);
      }
      return xmlDocument;
    }

    private void SauveSolution(XmlElement xRoot, Solution solution)
    {
      XmlDocument xDoc = xRoot.OwnerDocument;
      XmlElement xSolution = xDoc.CreateElement("solution");
      xRoot.AppendChild(xSolution);
      XmlElement xPlateau = xDoc.CreateElement("plateau");
      xSolution.AppendChild(xPlateau);
      xPlateau.InnerText = SauvePlateau(solution.SituationInitiale);
      foreach (SolutionMouvement mouvement in solution.Mouvements)
      {
        XmlElement xMouvement = xDoc.CreateElement("mouvement");
        xSolution.AppendChild(xMouvement);
        xMouvement.SetAttribute("x", mouvement.Pierre.X.ToString());
        xMouvement.SetAttribute("y", mouvement.Pierre.Y.ToString());
        xMouvement.SetAttribute("dir", Common.EncodeDirection(mouvement.Direction));
      }
    }

    private string SauvePlateau(SituationRaw plateau)
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine();
      // Les points du plateau sont réputés ordonnés par y croissant, et pour chaque y, par x croissant
      int idxCur = 0;
      for (int y = plateau.Etendue.yMin; y <= plateau.Etendue.yMax; y++)
      {
        for (int x = plateau.Etendue.xMin; x <= plateau.Etendue.xMax; x++)
        {
          if (idxCur < plateau.Count)
          {
            (int x, int y, bool pierre) @case = plateau[idxCur];
            if (@case.y == y && @case.x == x)
            {
              if (@case.pierre) sb.Append('x');
              else sb.Append('o');
              idxCur++;
            }
            else sb.Append(' ');
          }
          else sb.Append(' ');
        }
        sb.AppendLine();
      }
      // Sans le dernier AppendLine(), le xml a une balise fermante </plateau> décalée à gauche
      // Avec lui, la balise est correctement alignée, mais il y a une ligne vide
      //      sb.AppendLine();

      return sb.ToString();
    }
  }
}
