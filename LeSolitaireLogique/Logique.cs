using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  // Recherche de solution du jeu de solitaire
  // Jeu de plateau
  public class Logique
  {
    // Les coordonnées des différentes cases du rectangle minimal contenant le plateau
    public CoordonneesStock CoordonneesStock;
    // Le plateau est l'ensemble des cases qui peuvent accueillir une pierre
    public Plateau Plateau;
    // Une situation est l'ensemble des pierres sur le plateau
    public Situation SituationInitiale;
    public Situation SituationCourante;
    private SituationsStock SituationsStock;
    private long[] NbSituationsTestees;
    // Il peut y avoir de nombreuses solutions, du fait des symétries du plateau initial, mais pas seulement
    // Certains mouvements peuvent permuter sans changer fondamentalement la solution.
    // On s'arrête à la 1ère solution trouvée.
    private Mvt Solution;

    // Constructeur
    public Logique(SituationRaw situationInitiale)
    {
      BuildPlateauEtSituationInitiale(situationInitiale);
      Plateau.CalculeSymetries(CoordonneesStock);
      Plateau.CalculeMouvementsPossibles(CoordonneesStock);
    }
    private void BuildPlateauEtSituationInitiale(SituationRaw situationInitiale)
    {
      Etendue etendue = Common.CalculeEtendueCentree(situationInitiale);
      CoordonneesStock = new CoordonneesStock(etendue);
      Plateau = new Plateau();
      SituationInitiale = new Situation();
      foreach ((int x, int y, bool presencePierre) item in situationInitiale)
      {
        Coordonnee coordonnee = CoordonneesStock.GetCoordonnee(item.x + etendue.xMin, item.y + etendue.yMin);
        bool pierre = item.presencePierre;
        Plateau.AddCase(coordonnee);
        if (pierre) SituationInitiale.AddPierre(coordonnee);
      }
      // RQ : le hashcode de SituationInitiale n'est pas calculé, mais on n'en a pas besoin, cette situation initiale n'ayant aucune raison d'être dans le stock.
    }

    // RechercheSolution
    public Mvt RechercheSolution()
    {
      Solution = null;
      NbSituationsTestees = new long[SituationInitiale.Count];
      SituationsStock = new SituationsStock(SituationInitiale.Count);
      SituationCourante = new Situation(SituationInitiale);
      foreach (var mvtPlateau in Plateau.MouvementsPossibles)
      {
        if (SituationCourante.MouvementPossible(mvtPlateau))
        {
          // Oui, oui : SituationInitiale.Count - SituationInitiale.Count==0
          NbSituationsTestees[0]++;
          Mvt mvt1 = new Mvt() { Parent = null, Depart = mvtPlateau.A, Saut = mvtPlateau.B, Arrivee = mvtPlateau.C };
          SituationCourante.DeplacePierre(mvt1, CoordonneesStock.Etendue);
          RechercheInterne(mvt1);
          SituationCourante.RestaurePierre(mvt1, CoordonneesStock.Etendue);
        }
      }

      return Solution;
    }

    private void RechercheInterne(Mvt mvt)
    {
      if (SituationConnue(SituationCourante))
      {
        return;
      }
      SituationsStock.Add(SituationCourante);
      if (SituationCourante.NbPierres == 1)
      {
        Solution = mvt;
        return;
      }
      List<(Mvt mvt, int compacite)> mvtPossibles = new List<(Mvt, int)>();
      foreach (var mvtPlateau in Plateau.MouvementsPossibles)
      {
        if (SituationCourante.MouvementPossible(mvtPlateau))
        {
          NbSituationsTestees[SituationInitiale.Count - SituationCourante.Count]++;
          Mvt mvtPossible = new Mvt() { Parent = mvt, Depart = mvtPlateau.A, Saut = mvtPlateau.B, Arrivee = mvtPlateau.C };
          mvtPossibles.Add((mvtPossible, 0));
        }
      }
      //ClasseMouvementsPossibles(mvtPossibles);
      foreach ((Mvt mvt, int compacite) mvtPossible in mvtPossibles)
      {
        SituationCourante.DeplacePierre(mvtPossible.mvt, CoordonneesStock.Etendue);
        RechercheInterne(mvtPossible.mvt);
        SituationCourante.RestaurePierre(mvtPossible.mvt, CoordonneesStock.Etendue);
        if (Solution != null)
        {
          return;
        }
      }
    }

    private bool SituationConnue(Situation situationNew)
    {
      if (SituationsStock.Contains(situationNew))
      {
        return true;
      }
      foreach (Situation situationSymetrique in Plateau.SituationsSymetriques(situationNew))
      {
        if (SituationsStock.Contains(situationSymetrique))
        {
          return true;
        }
      }
      return false;
    }


  }
}
