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
  public class LogiqueIncrementale
  {
    // Les coordonnées des différentes cases du rectangle minimal contenant le plateau
    public CoordonneesStock CoordonneesStock;
    // Le plateau est l'ensemble des cases qui peuvent accueillir une pierre
    public Plateau Plateau;
    // Une situation est l'ensemble des pierres sur le plateau
    public Situation SituationInitiale;
    public Situation SituationCourante;
    private SituationsStock SituationsStock;
    int NombreMinPierres;

    // Constructeur
    public LogiqueIncrementale(FileInfo file, int nbMouvements)
    {
      SituationRaw plateauComplet = Common.ChargeContenuFichierSituation(file);
      BuildPlateauEtSituationInitiale(plateauComplet);
      Plateau.CalculeSymetries(CoordonneesStock);
      Plateau.CalculeMouvementsPossibles(CoordonneesStock);
      // On recherche le nombre de situations qui contiennent ce NombreMinPierres
      NombreMinPierres = SituationInitiale.Count - nbMouvements;

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
    }

    public void RechercheIncrementale()
    {
      Stopwatch sw = Stopwatch.StartNew();
      SituationsStock = new SituationsStock(Plateau.Count);
      SituationCourante = new Situation(SituationInitiale);
      RechercheIncrementaleInternal();
      sw.Stop();
      Debug.Print($"{sw.Elapsed.TotalSeconds} secondes");
      Debug.Print(SituationsStock.ToString());
    }

    private void RechercheIncrementaleInternal()
    {
      if (SituationConnue(SituationCourante))
      {
        return;
      }
      SituationsStock.Add(SituationCourante);
      foreach (var mvtPlateau in Plateau.MouvementsPossibles)
      {
        if (SituationCourante.MouvementPossible(mvtPlateau))
        {
          Mvt mvt1 = new Mvt() { Parent = null, Depart = mvtPlateau.A, Saut = mvtPlateau.B, Arrivee = mvtPlateau.C };
          SituationCourante.DeplacePierre(mvt1, CoordonneesStock.Etendue);
          RechercheInterne(mvt1);
          SituationCourante.RestaurePierre(mvt1, CoordonneesStock.Etendue);
        }
      }
    }
    private void RechercheInterne(Mvt mvt)
    {
      if (SituationConnue(SituationCourante))
      {
        return;
      }
      SituationsStock.Add(SituationCourante);
      if (SituationCourante.NbPierres <= NombreMinPierres)
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
        RechercheInterne(mvtPossible.mvt);
        SituationCourante.RestaurePierre(mvtPossible.mvt, CoordonneesStock.Etendue);
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

  }
}
