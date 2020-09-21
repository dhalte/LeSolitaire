using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LeSolitaireLogique
{
  // Représente le contenu d'une fiche d'initialisation ou d'un fichier pilote
  public class Config
  {
    public int Nd { get; private set; }
    public int Nf { get; private set; }
    public long IdxReprise;
    // C'est cet objet qui contient l'étendue du plateau
    public SituationRaw PlateauRaw;
    public List<Solution> Solutions;
    public Config(FileInfo file)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.Load(file.FullName);
      ChargeConfig(xmlDocument);
    }
    public Config(string contenu)
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
      PlateauRaw = ChargePlateauRaw(plateau);
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
      solution.SituationInitialeRaw = ChargePlateauRaw(xSituation.InnerText);
      solution.Mouvements = new List<SolutionMouvement>();
      foreach (XmlElement xMouvement in xSolution.SelectNodes("mouvement"))
      {
        byte idxPierre = byte.Parse(xMouvement.GetAttribute("c"));
        enumDirection direction = Common.DecodeDirection(xMouvement.GetAttribute("dir"));
        SolutionMouvement mouvement = new SolutionMouvement(idxPierre, direction);
        solution.Mouvements.Add(mouvement);
      }
      return solution;
    }

    private SituationRaw ChargePlateauRaw(string description)
    {
      return Common.ChargeSituationRaw(description);
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
      xPlateau.InnerText = SauvePlateauRaw(PlateauRaw);
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
      xPlateau.InnerText = SauvePlateauRaw(solution.SituationInitialeRaw);
      foreach (SolutionMouvement mouvement in solution.Mouvements)
      {
        XmlElement xMouvement = xDoc.CreateElement("mouvement");
        xSolution.AppendChild(xMouvement);
        xMouvement.SetAttribute("c", mouvement.IdxPierre.ToString());
        xMouvement.SetAttribute("dir", Common.EncodeDirection(mouvement.Direction));
      }
    }

    private string SauvePlateauRaw(SituationRaw plateauRaw)
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine();
      // Les points du plateau sont réputés ordonnés par y croissant, et pour chaque y, par x croissant
      int idxCur = 0;
      Etendue etendue = Common.CalculeEtendue(plateauRaw);
      for (int y = 0; y < etendue.Hauteur; y++)
      {
        for (int x = 0; x < etendue.Largeur; x++)
        {
          if (idxCur < plateauRaw.Count)
          {
            (int x, int y, bool pierre) @case = plateauRaw[idxCur];
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
