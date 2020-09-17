using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class Situation : HashSet<Coordonnee>
  {
    public Situation() { }
    // RQ : on ne copie pas le hashcode lors de ce clonage, mais on n'en a pas besoin
    // on clone la situation pour la modifier par DeplacePierre, et à ce moment, le hashcode définitif sera calculé.
    public Situation(IEnumerable<Coordonnee> collection) : base(collection) { }
    public SituationPacked SituationCompacte;

    public void AddPierre(Coordonnee coordonnee)
    {
      this.Add(coordonnee);
    }
    public void DeplacePierre(Mvt mvt, Etendue etendue)
    {
      Remove(mvt.Depart);
      Remove(mvt.Saut);
      Add(mvt.Arrivee);
      CalculeSituationCompacte(etendue);
    }
    public int NbPierres { get => this.Count; }

    internal void RestaurePierre(Mvt mvt, Etendue etendue)
    {
      Add(mvt.Depart);
      Add(mvt.Saut);
      Remove(mvt.Arrivee);
      CalculeSituationCompacte(etendue);
    }

    internal void CalculeSituationCompacte(Etendue etendue)
    {
      SituationCompacte = new SituationPacked(this.Count);
      foreach (Coordonnee coordonnee in this)
      {
        SituationCompacte.Add(etendue.FromXY(coordonnee));
      }
      SituationCompacte.Sort();
    }

    internal bool MouvementPossible((Coordonnee A, Coordonnee B, Coordonnee C) mvtPlateau)
    {
      return Contains(mvtPlateau.A) && Contains(mvtPlateau.B) && !Contains(mvtPlateau.C);
    }
    internal bool MouvementInversePossible((Coordonnee A, Coordonnee B, Coordonnee C) mvtPlateau)
    {
      return !Contains(mvtPlateau.A) && !Contains(mvtPlateau.B) && Contains(mvtPlateau.C);
    }

    internal string ToString(Plateau plateau, CoordonneesStock coordonneesStock)
    {
      StringBuilder sb = new StringBuilder();
      for (int y = coordonneesStock.yMin; y <= coordonneesStock.yMax; y++)
      {
        for (int x = coordonneesStock.xMin; x <= coordonneesStock.xMax; x++)
        {
          Coordonnee coordonnee = coordonneesStock.GetCoordonnee(x, y);
          if (this.Contains(coordonnee))
          {
            sb.Append('x');
          }
          else if (plateau.Contains(coordonnee))
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
  }
}
