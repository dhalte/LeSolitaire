using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LeSolitaireLogique
{
  // Représente le contenu d'un fichier pilote
  public class Pilote
  {
    public int Nd { get; private set; }
    public int Nf { get; private set; }
    public long IdxReprise;
    public string DBname;
    // C'est cet objet qui contient l'étendue du plateau
    public SituationRaw PlateauRaw;
    public List<Solution> Solutions;
    public List<PreSolution> PreSolutions;
    public Pilote()
    {
      PlateauRaw = new SituationRaw();
      Solutions = new List<Solution>();
      PreSolutions = new List<PreSolution>();
    }
    public Pilote(FileInfo file)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.Load(file.FullName);
      ChargeConfig(xmlDocument);
    }
    // Pour les tests
    public Pilote(string contenu)
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
      DBname = xParam.GetAttribute("dbname");
      // C'est lors de l'initialisation du fichier pilote qu'on donne un nom à la base de données et qu'on la crée
      if (string.IsNullOrEmpty(DBname) || !LeSolitaireMySQL.Common.DBexists(DBname))
      {
        throw new ApplicationException("Base de données non définie");
      }
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
      PreSolutions = new List<PreSolution>();
      foreach (XmlElement xPreSolution in xRoot.SelectNodes("presolution"))
      {
        PreSolutions.Add(DecodePreSolution(xPreSolution));
      }
    }

    private Solution DecodeSolution(XmlElement xSolution)
    {
      Solution solution = new Solution();
      solution.Complete = bool.Parse(xSolution.GetAttribute("complete"));
      XmlElement xSituation = xSolution["plateau"];
      solution.SituationInitialeRaw = ChargePlateauRaw(xSituation.InnerText);
      foreach (XmlElement xMouvement in xSolution.SelectNodes("mouvement"))
      {
        byte idxDepart = byte.Parse(xMouvement.GetAttribute("d"));
        byte idxSaut = byte.Parse(xMouvement.GetAttribute("s"));
        SolutionMouvement mouvement = new SolutionMouvement(idxDepart, idxSaut);
        solution.Mouvements.Add(mouvement);
      }
      return solution;
    }

    private PreSolution DecodePreSolution(XmlElement xPreSolution)
    {
      PreSolution preSolution = new PreSolution();
      preSolution.IdxSD = int.Parse(xPreSolution.GetAttribute("idxSD"));
      preSolution.IdxSIlist = new List<int>();
      foreach (XmlElement xIdxSI in xPreSolution.SelectNodes("initial"))
      {
        preSolution.IdxSIlist.Add(int.Parse(xIdxSI.GetAttribute("idxSI")));
      }
      preSolution.Mouvements = new List<SolutionMouvement>();
      foreach (XmlElement xMouvement in xPreSolution.SelectNodes("mouvement"))
      {
        byte idxPierre=byte.Parse(xMouvement.GetAttribute("d")), idxSaut = byte.Parse(xMouvement.GetAttribute("s"));
        SolutionMouvement mouvement = new SolutionMouvement(idxPierre, idxSaut);
        preSolution.Mouvements.Add(mouvement);
      }
      return preSolution;
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
      xParametres.SetAttribute("dbname", DBname);
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
      foreach (PreSolution preSolution in PreSolutions)
      {
        SauvePreSolution(xRoot, preSolution);
      }
      return xmlDocument;
    }

    private void SauveSolution(XmlElement xRoot, Solution solution)
    {
      XmlDocument xDoc = xRoot.OwnerDocument;
      XmlElement xSolution = xDoc.CreateElement("solution");
      xRoot.AppendChild(xSolution);
      xSolution.SetAttribute("complete", solution.Complete.ToString().ToLower());
      XmlElement xPlateau = xDoc.CreateElement("plateau");
      xSolution.AppendChild(xPlateau);
      xPlateau.InnerText = SauvePlateauRaw(solution.SituationInitialeRaw);
      foreach (SolutionMouvement mouvement in solution.Mouvements)
      {
        XmlElement xMouvement = xDoc.CreateElement("mouvement");
        xSolution.AppendChild(xMouvement);
        xMouvement.SetAttribute("d", mouvement.IdxDepart.ToString());
        xMouvement.SetAttribute("s", mouvement.IdxSaut.ToString());
      }
    }

    private void SauvePreSolution(XmlElement xRoot, PreSolution preSolution)
    {
      XmlDocument xDoc = xRoot.OwnerDocument;
      XmlElement xPreSolution = xDoc.CreateElement("presolution");
      xRoot.AppendChild(xPreSolution);
      xPreSolution.SetAttribute("idxSD", preSolution.IdxSD.ToString());
      foreach (int idxSI in preSolution.IdxSIlist)
      {
        XmlElement xInitial = xDoc.CreateElement("initial");
        xPreSolution.AppendChild(xInitial);
        xInitial.SetAttribute("idxSI", idxSI.ToString());
      }
      foreach (SolutionMouvement mouvement in preSolution.Mouvements)
      {
        XmlElement xMouvement = xDoc.CreateElement("mouvement");
        xPreSolution.AppendChild(xMouvement);
        xMouvement.SetAttribute("d", mouvement.IdxDepart.ToString());
        xMouvement.SetAttribute("s", mouvement.IdxSaut.ToString());
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

    internal void Initialise(string descriptionPlateauInitial)
    {
      PlateauRaw = Common.ChargeSituationRaw(descriptionPlateauInitial);
      Nf = Nd = 0;
      IdxReprise = 0;
      Solutions.Clear();
      PreSolutions.Clear();
      DBname = InitDB();
    }

    private string InitDB()
    {
      return LeSolitaireMySQL.Common.InitDB();
    }

    internal void ChangeND(int nd)
    {
      Nd = nd;
      PreSolutions.Clear();      
    }

    internal void ChangeNF(int nf)
    {
      Nf = nf;
      PreSolutions.Clear();
    }

    internal void IncrementerND()
    {
      Nd++;
      PreSolutions.Clear();
    }
    internal void IncrementerNF()
    {
      Nf++;
      PreSolutions.Clear();
    }
  }
}
