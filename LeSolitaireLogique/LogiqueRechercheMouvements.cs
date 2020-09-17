using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Schema;

namespace LeSolitaireLogique
{
  public class LogiqueRechercheMouvements:Logique
  {
    // Le plateau est l'ensemble des cases qui peuvent accueillir une pierre
    // Une situation est l'ensemble des pierres sur le plateau
    private SituationsStock SituationsStock;
    private Mvt Solution;

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
    }


    private bool SituationConnue(Situation situationNew)
    {
      if (SituationsStock.Contains(situationNew))
      {
        return true;
      }
      foreach (Situation situationSymetrique in Plateau.SituationsSymetriques(situationNew))
      {
        situationSymetrique.CalculeSituationCompacte(CoordonneesStock.Etendue);
        if (SituationsStock.Contains(situationSymetrique))
        {
          return true;
        }
      }
      return false;
    }
    private bool SituationFinaleX(Situation situationNew)
    {
      if (SituationFinale.SituationCompacte.Equals(situationNew.SituationCompacte))
      {
        return true;
      }
      foreach (Situation situationSymetrique in Plateau.SituationsSymetriques(situationNew))
      {
        situationSymetrique.CalculeSituationCompacte(CoordonneesStock.Etendue);
        if (SituationFinale.SituationCompacte.Equals(situationSymetrique.SituationCompacte))
        {
          return true;
        }
      }
      return false;
    }

    // Utilitaire pour retrouver une suite de mouvements qui mène d'une situation initiale à une situation intermédiaire
    public Situation SituationFinale;

    public LogiqueRechercheMouvements(SituationRaw situationInitiale) : base(situationInitiale)
    {
    }

    public Mvt RechercheMouvements(string situationFinaleString)
    {
      SituationRaw plateauFinal = Common.ChargeContenuStringSituation(situationFinaleString);

      BuildSituationFinale(plateauFinal);

      Solution = null;
      SituationsStock = new SituationsStock(SituationInitiale.Count);
      SituationFinale.CalculeSituationCompacte(CoordonneesStock.Etendue);
      SituationsStock.Add(SituationFinale);
      SituationCourante = new Situation(SituationInitiale);
      foreach (var mvtPlateau in Plateau.MouvementsPossibles)
      {
        if(Solution != null)
        {
          break;
        }
        if (SituationCourante.MouvementPossible(mvtPlateau))
        {
          Mvt mvt1 = new Mvt() { Parent = null, Depart = mvtPlateau.A, Saut = mvtPlateau.B, Arrivee = mvtPlateau.C };
          SituationCourante.DeplacePierre(mvt1, CoordonneesStock.Etendue);
          RechercheInterneMvt(mvt1);
          SituationCourante.RestaurePierre(mvt1, CoordonneesStock.Etendue);
        }
      }

      return Solution;
    }

    private void BuildSituationFinale(SituationRaw situationFinale)
    {
      Etendue etendue = Common.CalculeEtendueCentree(situationFinale);
      if (etendue.xMin != CoordonneesStock.xMin || etendue.xMax != CoordonneesStock.xMax ||
         etendue.yMin != CoordonneesStock.yMin || etendue.yMax != CoordonneesStock.yMax)
      {
        throw new ApplicationException("BuildSituationFinale : Erreur de décodage de la situation finale");
      }
      SituationFinale = new Situation();
      int nbCases = 0;
      foreach ((int x, int y, bool presencePierre) item in situationFinale)
      {
        Coordonnee coordonnee = CoordonneesStock.GetCoordonnee(item.x + etendue.xMin, item.y + etendue.yMin);
        bool pierre = item.presencePierre;
        if (!Plateau.Contains(coordonnee))
        {
          throw new ApplicationException("BuildSituationFinale : Erreur de décodage de la situation finale");
        }
        nbCases++;
        if (pierre) SituationFinale.AddPierre(coordonnee);
      }
      if (nbCases != Plateau.Count)
      {
        throw new ApplicationException("BuildSituationFinale : Erreur de décodage de la situation finale");
      }

    }

    private void RechercheInterneMvt(Mvt mvt)
    {
      if (Solution != null)
      {
        return;
      }
      if (SituationCourante.NbPierres == SituationFinale.NbPierres)
      {
        if (SituationFinaleX(SituationCourante))
        {
          Solution = mvt;
          return;
        }
      }
      if (SituationConnue(SituationCourante))
      {
        return;
      }
      SituationsStock.Add(SituationCourante);
      if (SituationCourante.NbPierres <= SituationFinale.NbPierres)
      {
        return;
      }
      List<(Mvt mvt, int compacite)> mvtPossibles = new List<(Mvt, int)>();
      foreach (var mvtPlateau in Plateau.MouvementsPossibles)
      {
        if (SituationCourante.MouvementPossible(mvtPlateau))
        {
          Mvt mvtPossible = new Mvt() { Parent = mvt, Depart = mvtPlateau.A, Saut = mvtPlateau.B, Arrivee = mvtPlateau.C };
          mvtPossibles.Add((mvtPossible, 0));
        }

      }
      foreach ((Mvt mvt, int compacite) mvtPossible in mvtPossibles)
      {
        SituationCourante.DeplacePierre(mvtPossible.mvt, CoordonneesStock.Etendue);
        RechercheInterneMvt(mvtPossible.mvt);
        SituationCourante.RestaurePierre(mvtPossible.mvt, CoordonneesStock.Etendue);
      }
    }
  }
}
