using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class Etendue
  {
    // A la différence de Rectangle, les bornes supérieures sont contenues
    public Etendue(int xMin, int xMax, int yMin, int yMax)
    {
      this.xMin = xMin;
      this.xMax = xMax;
      this.yMin = yMin;
      this.yMax = yMax;
    }
    public readonly int xMin;
    public readonly int xMax;
    public readonly int yMin;
    public readonly int yMax;
    public bool Contains(int x, int y) => xMin <= x && x <= xMax && yMin <= y && y <= yMax;
    //Indice case dans le rectangle minimal contenant le plateau, à partir de 0
    public byte FromXY(int x, int y) => (byte)(x - xMin + (xMax - xMin + 1) * (y - yMin));
    // coordonnées casedans le rectangle minimal contenant le plateau, à partir de (xMin, yMin)
    public (int x, int y) FromByte(byte i) => (i % (xMax - xMin + 1) + xMin, i / (xMax - xMin + 1) + yMin);

    public byte FromXY(Coordonnee coordonnee) => FromXY(coordonnee.X, coordonnee.Y);

  }
}
