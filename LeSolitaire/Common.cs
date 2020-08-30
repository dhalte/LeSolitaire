using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaire
{
  class Common
  {
    // Le plateau est décrit de façon textuelle ainsi :
    // chaque ligne de texte est une ligne du plateau
    // chaque espace est l'absence de case, 
    // chaque x est la présence d'une pierre
    // chaque o est une case vide
    public static List<(int, int, bool)> ChargeContenuFichierSituationInitiale(FileInfo file)
    {
      string contenuFichier = File.ReadAllText(file.FullName);
      string[] lignes = contenuFichier.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
      List<(int x, int y, bool presencePierre)> contenu = new List<(int, int, bool)>();
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
  }
}
