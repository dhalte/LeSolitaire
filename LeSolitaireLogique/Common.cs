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

    // Pour limiter les accès au disque, on choisit parmi toutes les symétries l'une d'entre elles de manière univoque
    // On assume que les deux tableaux sont définis, de mêmes tailles.
    // Est de poids moindre le premier tableau ayant une case vide alors que l'autre a une pierre
    internal static int Compare(bool[] pierres1, bool[] pierres2)
    {
      int l = pierres1.Length;
      for (int idx = 0; idx < l; idx++)
      {
        int n = pierres1[idx].CompareTo(pierres2[idx]);
        if (n != 0)
        {
          return n;
        }
      }
      return 0;
    }

    public static void Convert(bool[] pierres, byte[] idxPierres)
    {
      int idxPierre = 0;
      int lenBool = pierres.Length;
      for (byte idxInBool = 0; idxInBool < lenBool; idxInBool++)
      {
        if (pierres[idxInBool])
        {
          idxPierres[idxPierre++] = idxInBool;
        }
      }
    }

    // Construit un tableau de l'emplacement des pierres (donc n'affiche pas les cases vides)
    // et vérifie que idxPierres est trié
    public static string Dump(byte[] idxPierres, Plateau plateau)
    {
      int largeurPlateau = plateau.Etendue.Largeur;
      int hauteurPlateau = plateau.Etendue.Hauteur;
      StringBuilder sb = new StringBuilder();
      int idxInIdxPierres = 0;
      int lgIdxPierres = idxPierres.Length;
      for (int y = 0; y < hauteurPlateau; y++)
      {
        for (int x = 0; x < largeurPlateau; x++)
        {
          byte idxPlateau = plateau.Etendue.FromXY(x, y);
          if (idxInIdxPierres < lgIdxPierres && idxPierres[idxInIdxPierres] == idxPlateau)
          {
            if (idxInIdxPierres > 0 && idxPierres[idxInIdxPierres - 1] >= idxPierres[idxInIdxPierres])
            {
              throw new ApplicationException("le tableau n'est pas trié");
            }
            sb.Append('x');
            idxInIdxPierres++;
          }
          else if (plateau.Contains(idxPlateau))
          {
            sb.Append('o');
          }
          else
          {
            sb.Append(' ');
          }
        }
        sb.AppendLine();
      }
      return sb.ToString();
    }

    // les situations initiales et gagnantes sont numérotées de 0 à 7
    // et sont représentées par des bits levés ou non dans idxSG
    // On construit une chaine 01234567 où les entiers associés à un bit à 0 sont remplacés par un espace.
    internal static string ConvertFlags(byte idxSG)
    {
      string result = string.Empty;
      byte flag = 1;
      for (int i = 0; i < 8; i++)
      {
        if ((idxSG & flag) == flag)
        {
          result += i.ToString();
        }
        else
        {
          result += ' ';
        }
        flag <<= 1;
      }
      return result;
    }
  }
}
