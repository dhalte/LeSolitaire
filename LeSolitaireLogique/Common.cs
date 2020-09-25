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

    // Renvoie une liste des cases vides ou contenant une pierre trouvées dans la description
    // Le rectangle englobant est réglé pour que les coordonnées minimales des cases soit 0
    public static SituationRaw ChargeSituationRaw(string description)
    {
      int xMin = int.MaxValue, xMax = int.MinValue, yMin = int.MaxValue, yMax = int.MinValue;
      string[] lignes = description.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
      SituationRaw situationRaw = new SituationRaw();
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
              situationRaw.Add((i, l, true));
              break;
            case 'o':
              situationRaw.Add((i, l, false));
              break;
            default:
              throw new ApplicationException($"Caractère incorrect en ligne {l + 1}, position {i + 1}");
          }
          switch (c)
          {
            case 'x':
            case 'o':
              if (xMin > i) xMin = i;
              if (xMax < i) xMax = i;
              if (yMin > l) xMin = l;
              if (yMax < l) xMax = l;
              break;
          }
        }
      }
      if (situationRaw.Count == 0)
      {
        throw new ApplicationException("La description du plateau ne contient aucune pierre, aucune case");
      }
      if (xMin != 0 || yMin != 0)
      {
        situationRaw.ForEach(c => { c.x -= xMin; c.y -= yMin; });
      }
      return situationRaw;
    }

    // La situation est déjà réglée pour que les coordonnées minimales soient 0
    public static Etendue CalculeEtendue(SituationRaw liste)
    {
      int xMax = int.MinValue, yMax = int.MinValue;
      liste.ForEach(c =>
      {
        if (c.x > xMax) xMax = c.x;
        if (c.y > yMax) yMax = c.y;
      });
      return new Etendue(xMax + 1, yMax + 1);
    }

  }
}
