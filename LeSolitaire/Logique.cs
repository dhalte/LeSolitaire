using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaire
{
  // Recherche de solution du jeu de solitaire
  // Jeu de plateau
  class Logique
  {
    private int xMin, xMax, yMin, yMax;
    // Le plateau est l'ensemble des cases qui peuvent accueillir une pierre
    public Plateau Plateau;
    // Une situation est l'ensemble des pierres sur le plateau
    public Situation SituationInitiale;
    private Situation SituationCourante;
    private long[] NbSituationsTestees;
    // Il peut y avoir de nombreuses solutions, du fait des symétries du plateau initial, mais pas seulement
    // Certains mouvements peuvent permuter sans changer fondamentalement la solution.
    private Mvt Solution;
    List<Tuple<int, int, bool>> x;

    public Logique(List<(int x, int y, bool presencePierre)> situationInitiale)
    {
      BuildPlateau(situationInitiale);
    }
    public Mvt RechercheSolution()
    {
      Solution = null;
      NbSituationsTestees = new long[SituationInitiale.Count];
      RechercheInterne(null);
      return Solution;
    }

    private void BuildPlateau(List<(int x, int y, bool presencePierre)> situationInitiale)
    {
      xMin = yMin = int.MaxValue;
      xMax = yMax = int.MinValue;
      foreach ((int x, int y, bool presencePierre) item in situationInitiale)
      {
        if (xMin > item.x) xMin = item.x;
        if (xMax < item.x) xMax = item.x;
        if (yMin > item.y) yMin = item.y;
        if (yMax < item.y) yMax = item.y;
      }
      int xCentre = (xMin + xMax) / 2;
      int yCentre = (yMin + yMax) / 2;
      // Toutes les cases sont situées dans le rectangle ci dessous
      xMin -= xCentre;
      xMax -= xCentre;
      yMin -= yCentre;
      yMax -= yCentre;
      Coordonnees.InitStock(xMin, xMax, yMin, yMax);
      Plateau = new Plateau();
      SituationInitiale = new Situation();
      foreach ((int x, int y, bool presencePierre) item in situationInitiale)
      {
        Coordonnee coordonnee = Coordonnees.GetCoordonnee(item.x + xMin, item.y + yMin);
        bool pierre = item.presencePierre;
        Plateau.AddCase(coordonnee);
        if (pierre) SituationInitiale.AddPierre(coordonnee);
      }
      SituationCourante = new Situation(SituationInitiale);
    }
    //Les coordonnées ne sont pas encore générées, et celles ci-dessous pourraient mêmes ne pas appartenir au stock
    private static readonly Coordonnee[] Directions = { new Coordonnee(1, 0), new Coordonnee(-1, 0), new Coordonnee(0, 1), new Coordonnee(0, -1) };

    private void RechercheInterne(Mvt mvt)
    {
      if (Solution != null)
      {
        return;
      }
      if (mvt != null)
      {
        SituationCourante.DeplacePierre(mvt.Depart, mvt.Saut, mvt.Arrivee);
      }
      if (SituationCourante.NbPierres == 1)
      {
        Solution = mvt;
      }
      else
      {
        foreach (Coordonnee depart in SituationCourante.ToArray())
        {
          foreach (Coordonnee offset in Directions)
          {
            //if (depart.X == -2 && depart.Y == 0 && offset.X == 1 && offset.Y == 0)
            //{
            //  Debugger.Break();
            //}
            if (SituationCourante.MouvementPossible(Plateau, depart, offset))
            {
              NbSituationsTestees[SituationInitiale.Count - SituationCourante.Count]++;
              Coordonnee saut = depart.Offset(offset.X, offset.Y);
              Coordonnee arrivee = saut.Offset(offset.X, offset.Y);
              Mvt mvt1 = new Mvt() { Parent = mvt, Depart = depart, Saut = saut, Arrivee = arrivee };
              RechercheInterne(mvt1);
            }

          }
        }
        if (mvt != null)
        {
          SituationCourante.RestaurePierre(mvt.Depart, mvt.Saut, mvt.Arrivee);
        }
      }

    }
  }
}
