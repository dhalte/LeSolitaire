using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaire
{
  public class Situation : HashSet<Coordonnee>
  {
    public Situation() { }
    public Situation(IEnumerable<Coordonnee> collection) : base(collection) { }

    public void AddPierre(Coordonnee coordonnee)
    {
      this.Add(coordonnee);
    }
    public void DeplacePierre(Coordonnee depart, Coordonnee saut, Coordonnee arrivee)
    {
      //Situation resultat = new Situation(this);
      //resultat.Remove(depart);
      //resultat.Remove(saut);
      //resultat.Add(arrivee);
      //return resultat;
      Remove(depart);
      Remove(saut);
      Add(arrivee);
    }
    public int NbPierres { get => this.Count; }

    internal void RestaurePierre(Coordonnee depart, Coordonnee saut, Coordonnee arrivee)
    {
      Add(depart);
      Add(saut);
      Remove(arrivee);
    }

    internal bool MouvementPossible(Plateau plateau, Coordonnee depart, Coordonnee offset)
    {
      // depart : pierre D présente sur le plateau
      // offet : direction dans laquelle on veut la déplacer
      // testons la présence de la pierre S à coté de D 
      int x = depart.X + offset.X, y = depart.Y + offset.Y;
      // les coordonnées sont elles dans le rectangle dans lequel est inscrit le plateau ?
      if (!Coordonnees.Contains(x, y)) return false;
      Coordonnee coordonnee = Coordonnees.GetCoordonnee(x, y);
      // Le plateau a-t-il actuellement une pierre à cet emplacement ?
      if (!this.Contains(coordonnee)) return false;
      // testons l'emplacement A où on veut déplacer D
      x += offset.X; y += offset.Y;
      // les coordonnées sont elles dans le rectangle dans lequel est inscrit le plateau ?
      if (!Coordonnees.Contains(x, y)) return false;
      coordonnee = Coordonnees.GetCoordonnee(x, y);
      // A est-il dans le plateau ?
      if (!plateau.Contains(coordonnee)) return false;
      // A est-il vide ?
      if (this.Contains(coordonnee)) return false;
      return true;
    }
  }
}
