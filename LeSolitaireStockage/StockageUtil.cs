using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireStockage
{
  public partial class Stockage
  {
    // Pour débug, sur des arbres de tailles "modestes"
    public string Dump()
    {
      StringBuilder sb = new StringBuilder();
      Dump(Racine, Profondeur, sb);
      return sb.ToString();
    }
    private void Dump(StockageNoeud noeud, int profondeur, StringBuilder sb)
    {
      if (!noeud.IsFeuille)
      {
        StockageNoeud noeudEnfant = GetNoeudEnfant(noeud, profondeur, 0);
        Dump(noeudEnfant, profondeur - 1, sb);
      }
      for (int idxElt = 0; idxElt < noeud.NbElements; idxElt++)
      {
        Dump(noeud.data, idxElt, TailleElement, profondeur, sb);
        if (!noeud.IsFeuille)
        {
          StockageNoeud noeudEnfant = GetNoeudEnfant(noeud, profondeur, idxElt + 1);
          Dump(noeudEnfant, profondeur - 1, sb);
        }
      }
    }
    // format du dump d'un élément :
    // <niveau:00>,<idxElt:00><3*(1+niveau) espaces><Dump(Elt[idxElt])><crlf>
    // l'élément est converti en string par la classe de rappel
    private unsafe void Dump(byte[] data, int idxElt, int tailleElement, int profondeur, StringBuilder sb)
    {
      sb.Append($"{profondeur}".PadLeft(2, '0')).Append(',').Append($"{idxElt}".PadLeft(2, '0')).Append("".PadLeft(3 * (1 + Profondeur - profondeur)));
      fixed (byte* pElement = &data[1 + tailleElement * idxElt])
      {
        sb.Append(Comparateur.ToString(pElement));
      }
      sb.AppendLine();
    }

    private unsafe void Verifie(StockageNoeud noeud, int profondeur, ref StockageNoeud noeudPrecedent, ref int idxElementPrecedent)
    {
      if (!noeud.IsFeuille)
      {
        StockageNoeud noeudEnfant = GetNoeudEnfant(noeud, profondeur, 0);
        Verifie(noeudEnfant, profondeur - 1, ref noeudPrecedent, ref idxElementPrecedent);
      }
      for (int idxElt = 0; idxElt < noeud.NbElements; idxElt++)
      {
        if (noeudPrecedent != null)
        {
          fixed (byte* pElementPrecedent = &noeudPrecedent.data[1 + TailleElement * idxElementPrecedent], pElementCourant = &noeud.data[1 + TailleElement * idxElt])
          {
            int nCmp = Comparateur.CompareSituations(pElementPrecedent, pElementCourant);
            if (nCmp >= 0)
            {
              throw new ApplicationException();
            }
          }
        }
        noeudPrecedent = noeud;
        idxElementPrecedent = idxElt;
        if (!noeud.IsFeuille)
        {
          StockageNoeud noeudEnfant = GetNoeudEnfant(noeud, profondeur, idxElt + 1);
          Verifie(noeudEnfant, profondeur - 1, ref noeudPrecedent, ref idxElementPrecedent);
        }
      }
    }

    internal unsafe string Dump(StockageNoeud noeud)
    {
      StringBuilder sb = new StringBuilder();
      int nbElt = noeud.NbElements;
      sb.Append($"nb elt : {nbElt}").AppendLine();
      fixed (byte* pData = &noeud.data[1])
      {
        for (int idxElt = 0; idxElt < nbElt; idxElt++)
        {
          sb.Append(Comparateur.ToString(pData + idxElt * TailleElement)).AppendLine();
        }
      }
      return sb.ToString();
    }
    private class DataParcourt
    {
      internal StockageNoeud noeud;
      internal int idxElt;

      public DataParcourt(StockageNoeud noeud)
      {
        this.noeud = noeud;
        idxElt = 0;
      }
    }
    public IEnumerable<byte[]> EnumereElements()
    {
      // Cas particulier de l'arbre vide
      if (Racine.NbElements == 0)
      {
        yield break;
      }
      // Initialisation des structures permettant le parcours de l'arbre
      List<DataParcourt> pile = new List<DataParcourt>();
      int idxProfondeur, idxElt;
      StockageNoeud noeud = Racine;
      pile.Add(new DataParcourt(noeud));
      for (idxProfondeur = Profondeur; idxProfondeur > 0; idxProfondeur--)
      {
        noeud = GetNoeudEnfant(noeud, idxProfondeur, 0);
        pile.Add(new DataParcourt(noeud));
      }
      // buffer permettant le renvoi de l'élément à chacun des appels de cet itérateur
      byte[] result = new byte[TailleElement];
      // A partir d'ici, idxProfondeur est l'inverse de la "profondeur" utilisée par ailleurs.
      // On pointe le dernier noeud de Pile, qui est une feuille
      idxProfondeur = Profondeur;

      // Boucle qui sera interrompue :
      //  sur un yield return qui délivre un élément. L'exécution reprendra à cet endroit, avec les variables locales inchangées
      //  lorsque le dernier élément aura été délivré. L'algorithme aura alors positionné idxProfondeur à -1
      while (idxProfondeur >= 0)
      {
        noeud = pile[idxProfondeur].noeud;
        idxElt = pile[idxProfondeur].idxElt;
        // L'élément qu'on va renvoyer
        Array.Copy(noeud.data, 1 + TailleElement * idxElt, result, 0, TailleElement);
        if (idxProfondeur == Profondeur)
        {
          // On est dans une feuille
          if (idxElt < noeud.NbElements - 1)
          {
            // il y a encore des éléments à renvoyer dans cette feuille
            pile[idxProfondeur].idxElt++;
          }
          else
          {
            // Feuille épuisée, on remonte la liste des noeuds
            for (idxProfondeur = idxProfondeur - 1; idxProfondeur >= 0; idxProfondeur--)
            {
              noeud = pile[idxProfondeur].noeud;
              idxElt = pile[idxProfondeur].idxElt;
              if (idxElt < noeud.NbElements)
              {
                // Au prochain appel c'est cet enfant Pile[idxProfondeur].noeud.Enfants[Pile[idxProfondeur].idxElt] qui sera renvoyé
                break;
              }
            }
            // Si on n'a pas trouvé de noeud disponible, idxProfondeur est -1, on va retourner result qui est le plus grand élément,
            // et à la prochaine itération, cette condition idxProfondeur < 0 interrompra la boucle principale
          }
        }
        else
        {
          // On n'est pas dans une feuille
          // La prochaine fois qu'on rencontrera ce noeud, c'est son élément suivant qu'on renverra
          // sauf si idxElt == nbElts, dans quel cas on ira chercher plus haut dans la pile
          idxElt = ++pile[idxProfondeur].idxElt;
          for (; idxProfondeur < Profondeur; idxProfondeur++)
          {
            // A la première itération, l'enfant est celui qui contient les éléments juste supérieurs à result
            // mais les fois suivantes, on prend l'enfant de gauche.
            pile[idxProfondeur + 1].noeud = noeud = GetNoeudEnfant(noeud, Profondeur - idxProfondeur, idxElt);
            // et à chaque itération, on prend toujours l'élément de gauche de l'enfant.
            pile[idxProfondeur + 1].idxElt = idxElt = 0;
          }
          // A la sortie de la boucle, idxProfondeur == Profondeur : on pointe une feuille de l'arbre
        }
        yield return result;
      }
    }

    public class StatsProfondeur
    {
      public int profondeur;
      public int nbNoeuds;
      public int nbElements;
    }
    public List<StatsProfondeur> CollecteStats()
    {
      List<StatsProfondeur> stats = new List<StatsProfondeur>();
      for (int idxProfondeur = Profondeur; idxProfondeur >= 0; idxProfondeur--)
      {
        stats.Add(new StatsProfondeur());
      }
      CollecteStats(stats, Profondeur, Racine);
      return stats;
    }

    private void CollecteStats(List<StatsProfondeur> stats, int idxProfondeur, StockageNoeud noeud)
    {
      StatsProfondeur s = stats[idxProfondeur];
      s.nbNoeuds++;
      s.nbElements += noeud.NbElements;
      if (!noeud.IsFeuille)
      {
        CollecteStats(stats, idxProfondeur - 1, GetNoeudEnfant(noeud, idxProfondeur, 0));
        for (int idxElt = 0; idxElt < noeud.NbElements; idxElt++)
        {
          CollecteStats(stats, idxProfondeur - 1, GetNoeudEnfant(noeud, idxProfondeur, idxElt+1));
        }
      }
    }
  }
}
