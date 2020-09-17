using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class Common
  {
    // Le plateau est décrit de façon textuelle ainsi :
    // chaque ligne de texte est une ligne du plateau
    // chaque espace est l'absence de case, 
    // chaque x est la présence d'une pierre
    // chaque o est une case vide
    public static SituationRaw ChargeContenuFichierSituation(FileInfo file)
    {
      string contenuFichier = File.ReadAllText(file.FullName);
      return ChargeContenuStringSituation(contenuFichier);
    }

    public static SituationRaw ChargeContenuStringSituation(string description)
    {
      string[] lignes = description.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
      SituationRaw contenu = new SituationRaw();
      for (int l = 0; l < lignes.Length; l++)
      {
        string ligne = lignes[l];
        for (int i = 0; i < ligne.Length; i++)
        {
          char c = Char.ToLower(ligne[i]);
          switch (c)
          {
            case ' ':
              break;
            case 'x':
              contenu.Add((i, l, true));
              break;
            case 'o':
              contenu.Add((i, l, false));
              break;
            default:
              throw new ApplicationException($"Caractère incorrect en ligne {l + 1}, position {i + 1}");
          }
        }
      }
      return contenu;
    }

    public static Etendue CalculeEtendue(SituationRaw liste)
    {
      int xMin = int.MaxValue; int xMax = int.MinValue; int yMin = int.MaxValue; int yMax = int.MinValue;
      liste.ForEach(c =>
      {
        if (c.x < xMin) xMin = c.x;
        if (c.y < yMin) yMin = c.y;
        if (c.x > xMax) xMax = c.x;
        if (c.y > yMax) yMax = c.y;
      });
      return new Etendue(xMin, xMax, yMin, yMax);
    }
    public static Etendue CalculeEtendueCentree(SituationRaw liste)
    {
      Etendue etendue = Common.CalculeEtendue(liste);
      int xCentre = (etendue.xMin + etendue.xMax) / 2;
      int yCentre = (etendue.yMin + etendue.yMax) / 2;
      // Toutes les cases sont situées dans le rectangle ci dessous
      etendue = new Etendue(etendue.xMin - xCentre, etendue.xMax - xCentre, etendue.yMin - yCentre, etendue.yMax - yCentre);
      return etendue;
    }

    public static enumDirection DecodeDirection(string code)
    {
      if (string.IsNullOrEmpty(code) || code.Length != 1)
        throw new ArgumentException($"Code de direction invalide : {code} ");

      switch (char.ToLower(code[0]))
      {
        case 'n': return enumDirection.nord;
        case 'e': return enumDirection.est;
        case 's': return enumDirection.sud;
        case 'o': return enumDirection.ouest;
      }
      throw new ArgumentException($"Code de direction invalide : {code} ");
    }

    internal static string EncodeDirection(enumDirection direction)
    {
      switch (direction)
      {
        case enumDirection.nord:
          return "n";
        case enumDirection.est:
          return "e";
        case enumDirection.sud:
          return "s";
        case enumDirection.ouest:
          return "o";
      }
      throw new ArgumentException($"direction inconnue : {(int)direction} {direction}");
    }
  }
}
