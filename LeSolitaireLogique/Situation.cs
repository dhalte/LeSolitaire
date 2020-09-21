using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  // L'héritage est nécessaire pour l'appel à HashSet<SituationBase>.Add(Situation)
  public class Situation : SituationBase
  {
    public byte[] Pierres;

    public Situation(Etendue etendue, SituationRaw plateauRaw)
    {
      int len = plateauRaw.Count;
      List<byte> pierres = new List<byte>();
      for (byte idx = 0; idx < len; idx++)
      {
        (int x, int y, bool pierre) @case = plateauRaw[idx];
        if (@case.pierre)
        {
          pierres.Add(etendue.FromXY(@case.x, @case.y));
        }
      }
      Pierres = pierres.ToArray();
    }

    public Situation(bool[] pierresImages)
    {
      List<byte> pierres = new List<byte>();
      for (byte idx = 0; idx < pierresImages.Length; idx++)
      {
        if (pierresImages[idx])
        {
          pierres.Add(idx);
        }
      }
      Pierres = pierres.ToArray();
    }

    public Situation(byte[] situationRaw)
    {
      Pierres = new byte[situationRaw.Length];
      Array.Copy(situationRaw, Pierres, situationRaw.Length);
    }

    public int NbPierres => Pierres.Length;
    public override bool Equals(object obj)
    {
      int l = NbPierres;
      // On assume que 
      //  obj est toujours un objet de type Situation ou SituationEtude
      //  le tableau de this et obj est toujours alloué
      //  les deux tableaux ont même taille
      //  qu'ils sont triés par ordre croissant
      Situation situation = obj as Situation;
      if (situation != null)
      {
        for (int i = 0; i < NbPierres; i++)
        {
          if (Pierres[i] != situation.Pierres[i]) 
          {
            return false; 
          }
        }
        return true;
      }
      return ((SituationEtude)obj).Equals(this);
    }
    public override int GetHashCode()
    {
      int hashcode = 7;
      int l = NbPierres;
      unchecked
      {
        for (int i = 0; i < l; i++)
        {
          hashcode = 31 * hashcode + Pierres[i];
        }
      }
      return hashcode;
    }
    public string Dump(Plateau plateau)
    {
      StringBuilder sb = new StringBuilder();
      for (int y = 0; y < plateau.Etendue.Hauteur; y++)
      {
        for (int x = 0; x < plateau.Etendue.Largeur; x++)
        {
          bool bPlateau = plateau.Contains(x, y);
          byte idxCase = plateau.Etendue.FromXY(x, y);
          bool bPierre = Pierres.Contains(idxCase);
          char c = ' ';
          if (bPlateau)
          {
            c = bPierre ? 'x' : 'o';
          }
          else
          {
            if (bPierre)
            {
              c = 'X';
              Debugger.Break();
            }
          }
          sb.Append(c);
        }
        sb.AppendLine();
      }
      return sb.ToString();
    }
  }
}
