using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
  [TestClass]
  public class GenerationCode
  {
    private const string PlateauClassique = @"
  xxx
  xxx
xxxxxxx
xxxxxxx
xxxxxxx
  xxx
  xxx";
    private const string PlateauFrancais = @"
  xxx
 xxxxx
xxxxxxx
xxxxxxx
xxxxxxx
 xxxxx
  xxx";


    private string Plateau = PlateauClassique;
    private List<(int x, int y)> Coordonnees; // coordonnées des cases du plateau 
    // Contient l'indice de la case, de 0 à NbCases-1, ou -1 si ∉ plateau
    private int[,] Cases;
    private int Largeur, Hauteur, NbCases;
    private List<(int a, int b, int c)> MvtPossibles;
    private List<int[]> Transformations;

    [TestMethod]
    public void GenereMvtPossiblesEtTransformations()
    {
      AnalysePlateau();
      DresseListeMvtPossibles();
      GenereTransformations();
      StringBuilder sb = new StringBuilder();
      sb.AppendLine($"internal const int NbMouvementsAutorises = {MvtPossibles.Count};");
      sb.AppendLine("internal static readonly (int a, int b, int c)[] MouvementsAutorises = {");
      foreach (var item in MvtPossibles)
      {
        sb.AppendLine($"({item.a,2},{item.b,2},{item.c,2}),");
      }
      sb.AppendLine("};");
      sb.AppendLine("// Liste des transformations ");
      sb.AppendLine($"internal const int NbTransformations = {Transformations.Count};");
      foreach (int[] item in Transformations)
      {
        sb.Append("new int[] {");
        foreach (int idx  in item)
        {
          sb.Append($"{idx,2},");
        }
        sb.AppendLine("},");
      }
      sb.AppendLine("};");
      Debug.Print(sb.ToString());
    }

    private void DresseListeMvtPossibles()
    {
      MvtPossibles = new List<(int a, int b, int c)>();
      (int dx, int dy)[] shifts = { (0, -1), (1, 0), (0, 1), (-1, 0) };
      for (int idxA = 0; idxA < NbCases; idxA++)
      {
        (int x, int y) coordA = Coordonnees[idxA];
        foreach ((int dx, int dy) shift in shifts)
        {
          (int x, int y) coordB = (coordA.x + shift.dx, coordA.y + shift.dy);
          (int x, int y) coordC = (coordB.x + shift.dx, coordB.y + shift.dy);
          if (IsCasePlateau(coordB) && IsCasePlateau(coordC))
          {
            int idxB = IndiceCase(coordB);
            int idxC = IndiceCase(coordC);
            MvtPossibles.Add((idxA, idxB, idxC));
          }
        }
      }
    }

    // Indice de la case, de 0 à NbCases-1, en fonction de ses coordonnées
    private int IndiceCase((int x, int y) coord)
    {
      return Cases[coord.x, coord.y];
    }

    private bool IsCasePlateau((int x, int y) coord)
    {
      return 0 <= coord.x && coord.x < Largeur &&
             0 <= coord.y && coord.y < Hauteur &&
             Cases[coord.x, coord.y] >= 0;
    }
    private void AnalysePlateau()
    {
      // Au décodage, xMin, yMin peuvent être ≠ 0, mais on effectue ensuite un recadrage pour avoir xMin = xMax = 0
      int xMin = int.MaxValue, xMax = int.MinValue, yMin = int.MaxValue, yMax = int.MinValue;
      // Décodage du Plateau décrit par une chaine de caractères
      List<(int x, int y)> coordonnees = new List<(int x, int y)>();
      string[] lignes = Plateau.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
      for (int y = 0; y < lignes.Length; y++)
      {
        string ligne = lignes[y];
        for (int x = 0; x < ligne.Length; x++)
        {
          if (ligne[x] != ' ')
          {
            coordonnees.Add((x, y));
            if (xMin > x) xMin = x;
            if (xMax < x) xMax = x;
            if (yMin > y) yMin = y;
            if (yMax < y) yMax = y;
          }
        }
      }
      Largeur = 1 + xMax - xMin;
      Hauteur = 1 + yMax - yMin;
      NbCases = coordonnees.Count;
      Coordonnees = new List<(int x, int y)>();
      Cases = new int[Largeur, Hauteur];
      for (int y = 0; y < Hauteur; y++)
      {
        for (int x = 0; x < Largeur; x++)
        {
          Cases[x, y] = -1;
        }
      }
      for (int idxCase = 0; idxCase < NbCases; idxCase++)
      {
        (int x, int y) item = coordonnees[idxCase];
        int x = item.x - xMin, y = item.y - yMin;
        Coordonnees.Add((x, y));
        Cases[x, y] = idxCase;
      }
    }
    private void GenereTransformations()
    {
      Transformations = new List<int[]>();
      GenereIdentite();
      if (Largeur == Hauteur)
      {
        GenereRot90();
      }
      GenereRot180();
      if (Largeur == Hauteur)
      {
        GenereRot270();
      }
      GenereSymVerticale();
      GenereSymHorizontale();
      if (Largeur == Hauteur)
      {
        GenereSymDiagPrincipale();
        GenereSymDiagSecondaire();
      }
    }

    private void GenereSymDiagSecondaire()
    {
      int[] result = new int[NbCases];
      for (int i = 0; i < NbCases; i++)
      {
        (int x, int y) coord = Coordonnees[i];
        (int x, int y) img = (-coord.y + Hauteur - 1, -coord.x + Hauteur - 1);
        if (!IsCasePlateau(img))
        {
          return;
        }
        result[i] = Cases[img.x, img.y];
      }
      Transformations.Add(result);
    }

    private void GenereSymDiagPrincipale()
    {
      int[] result = new int[NbCases];
      for (int i = 0; i < NbCases; i++)
      {
        (int x, int y) coord = Coordonnees[i];
        (int x, int y) img = (coord.y, coord.x);
        if (!IsCasePlateau(img))
        {
          return;
        }
        result[i] = Cases[img.x, img.y];
      }
      Transformations.Add(result);
    }

    private void GenereSymHorizontale()
    {
      int[] result = new int[NbCases];
      for (int i = 0; i < NbCases; i++)
      {
        (int x, int y) coord = Coordonnees[i];
        (int x, int y) img = (coord.x, -coord.y + Hauteur - 1);
        if (!IsCasePlateau(img))
        {
          return;
        }
        result[i] = Cases[img.x, img.y];
      }
      Transformations.Add(result);
    }

    private void GenereSymVerticale()
    {
      int[] result = new int[NbCases];
      for (int i = 0; i < NbCases; i++)
      {
        (int x, int y) coord = Coordonnees[i];
        (int x, int y) img = (-coord.x + Largeur - 1, coord.y);
        if (!IsCasePlateau(img))
        {
          return;
        }
        result[i] = Cases[img.x, img.y];
      }
      Transformations.Add(result);
    }

    private void GenereRot270()
    {
      int[] result = new int[NbCases];
      for (int i = 0; i < NbCases; i++)
      {
        (int x, int y) coord = Coordonnees[i];
        (int x, int y) img = (coord.y, -coord.x + Hauteur - 1);
        if (!IsCasePlateau(img))
        {
          return;
        }
        result[i] = Cases[img.x, img.y];
      }
      Transformations.Add(result);
    }

    private void GenereRot180()
    {
      int[] result = new int[NbCases];
      for (int i = 0; i < NbCases; i++)
      {
        (int x, int y) coord = Coordonnees[i];
        (int x, int y) img = (-coord.x + Largeur - 1, -coord.y + Hauteur - 1);
        if (!IsCasePlateau(img))
        {
          return;
        }
        result[i] = Cases[img.x, img.y];
      }
      Transformations.Add(result);
    }

    private void GenereRot90()
    {
      int[] result = new int[NbCases];
      for (int i = 0; i < NbCases; i++)
      {
        (int x, int y) coord = Coordonnees[i];
        (int x, int y) img = (-coord.y + Hauteur - 1, coord.x);
        if (!IsCasePlateau(img))
        {
          return;
        }
        result[i] = Cases[img.x, img.y];
      }
      Transformations.Add(result);
    }

    private void GenereIdentite()
    {
      int[] result = new int[NbCases];
      for (int i = 0; i < NbCases; i++)
      {
        result[i] = i;
      }
      Transformations.Add(result);
    }
  }
}
