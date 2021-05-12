using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogiqueV0
{
  public class Etendue
  {
    // Les dimensions du rectangle englobant
    public Etendue(int largeur, int hauteur)
    {
      this.Largeur = largeur;
      this.Hauteur = hauteur;
      xCentre = Largeur / 2;
      yCentre = Hauteur / 2;
      NbCasesRectangleEnglobant = Largeur * Hauteur;
    }
    public readonly int Largeur;
    public readonly int Hauteur;
    public readonly int NbCasesRectangleEnglobant;
    // Celui utilisé pour convertir en coordonnées de part et d'autre de l'origine
    // Ne pas utiliser pour le calcul des symétries (quoique pour les plateaux classique et français, les deux coïncident)
    public readonly int xCentre;
    public readonly int yCentre;

    public bool Contains(int idxCase) => 0 <= idxCase && idxCase < NbCasesRectangleEnglobant;
    public bool Contains(int x, int y) => 0 <= x && x < Largeur && 0 <= y && y < Hauteur;
    
    //Indice case dans le rectangle englobant, à partir de 0.
    //Les coordonnées minimales sont 0
    public byte FromXY(int x, int y) => (byte)(x + Largeur * y);
    public byte FromXY((int x, int y) coordonnees) => (byte)(coordonnees.x + Largeur * coordonnees.y);

    //Indice case dans le rectangle minimal contenant le plateau, à partir de 0
    //Les coordonnées sont centrées
    public byte FromXYCentre(int x, int y) => (byte)((x + xCentre) + Largeur * (y + yCentre));
    
    // coordonnées case dans le rectangle englobant, les coordonnées minimales sont 0
    public (int x, int y) FromByte(byte i) => (i % Largeur, i / Largeur);

    // Fournir des coordonnées centrées autour de l'origine, pour une lecture plus facile des solutions.
    public (int x, int y) Centrer(int x, int y) => (x - xCentre, y - yCentre);
    public (int x, int y) Centrer((int x, int y) c) => (c.x - xCentre, c.y - yCentre);

    public byte IdxArrivee(byte idxDepart, byte idxSaut)
    {
      (int x, int y) depart = FromByte(idxDepart);
      (int x, int y) saut = FromByte(idxSaut);
      return FromXY(2 * saut.x - depart.x, 2 * saut.y - depart.y);
    }
  }
}
