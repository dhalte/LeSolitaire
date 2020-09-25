using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  // L'héritage est nécessaire pour l'appel à HashSet<SituationBase>.Contains(SituationEtude)
  public class SituationEtude : SituationBase
  {
    public Etendue Etendue;
    // La situation à l'étude.
    public bool[] Pierres;
    // la situation déduite par symétrie de la précédente.
    public bool[] ImagePierres;
    public int NbPierres { get; private set; }
    public SituationEtude(Plateau plateau)
    {
      Etendue = plateau.Etendue;      
      Pierres = new bool[Etendue.NbCasesRectangleEnglobant];
      ImagePierres = new bool[Etendue.NbCasesRectangleEnglobant];
    }
    public void ChargeSituationRaw(SituationRaw situationRaw)
    {
      Array.Clear(Pierres, 0, Pierres.Length);
      int len = situationRaw.Count;
      NbPierres = 0;
      for (int idx = 0; idx < len; idx++)
      {
        (int x, int y, bool pierre) @case = situationRaw[idx];
        if (@case.pierre)
        {
          int idxCase = Etendue.FromXY(@case.x, @case.y);
          Pierres[idxCase] = true;
          NbPierres++;
        }
      }
    }

    // On assume : 
    //   que obj est toujours du type Situation
    //   que les deux objets ont même nombre de pierres
    public override bool Equals(object obj)
    {
      Situation situation = obj as Situation;
      foreach (byte idxPierre in situation.Pierres)
      {
        if (!ImagePierres[idxPierre])
        {
          return false;
        }
      }
      return true;
    }
    // Ce mode de calcul donnera le même résultat que dans une situation 
    public override int GetHashCode()
    {
      int hashcode = 7;
      unchecked
      {
        for (int idx = 0; idx < ImagePierres.Length; idx++)
        {
          if (ImagePierres[idx])
          {
            hashcode = 31 * hashcode + idx;
          }
        }
      }
      return hashcode;
    }

    internal Situation NewSituation()
    {
      Situation situation = new Situation(Pierres);
      return situation;
    }

    public void ChargeSituation(Situation situation)
    {
      Array.Clear(Pierres, 0, Pierres.Length);
      NbPierres = situation.NbPierres;
      foreach (byte  idxCase in situation.Pierres)
      {
        Pierres[idxCase] = true;
      }
    }
    public void ChargeSituation(byte[] pierres)
    {
      Array.Clear(Pierres, 0, Pierres.Length);
      NbPierres = pierres.Length;
      foreach (byte idxCase in pierres)
      {
        Pierres[idxCase] = true;
      }
    }

    internal bool MouvementPossible((int idxOrigine, int idxVoisin, int idxDestination) mvt)
    {
      return Pierres[mvt.idxOrigine] && Pierres[mvt.idxVoisin] && !Pierres[mvt.idxDestination];
    }

    public void EffectueMouvement((byte idxOrigine, byte idxVoisin, byte idxDestination) mvt)
    {
      Pierres[mvt.idxOrigine] = false;
      Pierres[mvt.idxVoisin] = false;
      Pierres[mvt.idxDestination] = true;
      NbPierres--;
    }
    internal bool MouvementInversePossible((int idxOrigine, int idxVoisin, int idxDestination) mvt)
    {
      return !Pierres[mvt.idxOrigine] && !Pierres[mvt.idxVoisin] && Pierres[mvt.idxDestination];
    }

    internal void EffectueMouvementInverse((int idxOrigine, int idxVoisin, int idxDestination) mvt)
    {
      Pierres[mvt.idxOrigine] = true;
      Pierres[mvt.idxVoisin] = true;
      Pierres[mvt.idxDestination] = false;
      NbPierres++;
    }

    public string DumpPierres(Plateau plateau)
    {
      return Dump(plateau, Pierres);
    }
    public string DumpImagePierres(Plateau plateau)
    {
      return Dump(plateau, ImagePierres);
    }
    private string Dump(Plateau plateau, bool[] tbl)
    {
      StringBuilder sb = new StringBuilder();
      for (int y = 0; y < plateau.Etendue.Hauteur; y++)
      {
        for (int x = 0; x < plateau.Etendue.Largeur; x++)
        {
          bool bPlateau = plateau.Contains(x, y);
          byte idxCase = plateau.Etendue.FromXY(x, y);
          bool bPierre = tbl[idxCase];
          char c = ' ';
          if (bPlateau)
          {
            c = bPierre ? 'x' : 'o';
          }
          else
          {
            if (bPierre) { c = 'X'; Debugger.Break(); }
          }
          sb.Append(c);
        }
        sb.AppendLine();
      }
      return sb.ToString();
    }
  }
}
