using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTree
{
  public class BTreeVolatile
  {
    // taille en octets d'un élément
    protected readonly int TailleElement;
    // 1+nb max d'éléments, nb max liens vers noeuds enfants
    protected readonly int Ordre;
    // constante utilisée lors d'un split de noeud
    protected readonly int IdxPivot;
    // fonction de rappel pour comparaison d'éléments, et éventuelle mise à jour de la partie variable
    protected readonly IComparateurSituations Comparateur;
    internal NoeudVolatile Racine;
    // Profondeur == 0 pour les feuilles
    private int _Profondeur;
    protected virtual int Profondeur => _Profondeur;
    internal bool NoeudComplet(NoeudVolatile noeud) => noeud.data[0] == Ordre - 1;
    internal List<DataParcourtVolatile> PileInsertion;
    internal DataRemonteeVolatile dataRemontee, dataRemonteeSuivante;

    public BTreeVolatile(int tailleElement, int ordre, IComparateurSituations cmp)
    {
      TailleElement = tailleElement;
      Ordre = ordre;
      // division euclidienne
      IdxPivot = ordre / 2 - 1;
      Comparateur = cmp;
    }
    public virtual void InitBTree()
    {
      Racine = new NoeudVolatile(Ordre, TailleElement, true);
      _Profondeur = 0;
      PileInsertion = new List<DataParcourtVolatile>();
      PileInsertion.Add(new DataParcourtVolatile());
      dataRemontee = new DataRemonteeVolatile(TailleElement);
      dataRemonteeSuivante = new DataRemonteeVolatile(TailleElement);
    }
    public virtual unsafe InsertOrUpdateResult InsertOrUpdate(byte* pSituation)
    {
      // Cas particulier de l'arbre vide
      if (Racine.NbElements == 0)
      {
        Racine.Insert(pSituation, 0, TailleElement, new DataRemonteeVolatile(0));
        return InsertOrUpdateResult.Inserted;
      }
      // Recherche de la présence dans l'arbre de la situation passée en paramètre, avec mémoire du chemin parcouru pour la trouver
      int idxProfondeur;
      NoeudVolatile noeudActuel = Racine;
      for (idxProfondeur = Profondeur; idxProfondeur >= 0; idxProfondeur--)
      {
        bool bFound = RechercheDichotomique(pSituation, noeudActuel, out int idxPosition);
        if (bFound)
        {
          // La situation en paramètre est déjà présente dans l'arbre. idxPosition est alors >= 0
          fixed (byte* pSituationExistante = &noeudActuel.data[1 + idxPosition * TailleElement])
          {
            // On donne une chance à l'appelant de modifier les données variables de la situation stockée dans l'arbre
            //  param1 : situation passée en paramètre
            //  param2 : situation existante dans le B-Tree, et MajSituation renvoie true s'il modifie param2
            bool bModifie = Comparateur.MajSituation(pSituation, pSituationExistante);
            if (bModifie)
            {
              noeudActuel.dirty = true;
              return InsertOrUpdateResult.Updated;
            }
            return InsertOrUpdateResult.NoChange;
          }
        }
        // mise en mémoire du chemin suivi lors de la recherche de la situation passée en paramètre
        PileInsertion[idxProfondeur].noeud = noeudActuel;
        PileInsertion[idxProfondeur].idxElt = idxPosition;
        if (idxProfondeur > 0)
        {
          noeudActuel = GetNoeudEnfant(noeudActuel, idxProfondeur, idxPosition + 1);
        }
      }
      // Si on arrive ici, c'est qu'on n'a pas trouvé la situation passée en paramètre
      // alors le chemin suivi nous a mené à une feuille dans laquelle la situation doit prendre place
      for (int idxData = 0; idxData < TailleElement; idxData++)
      {
        dataRemontee.elementRemonte[idxData] = pSituation[idxData];
      }
      dataRemontee.ResetEnfant();
      for (idxProfondeur = 0; idxProfondeur <= Profondeur; idxProfondeur++)
      {
        DataParcourtVolatile pop = PileInsertion[idxProfondeur];
        if (!NoeudComplet(pop.noeud))
        {
          fixed (byte* pElementRemonte = dataRemontee.elementRemonte)
          {
            pop.noeud.Insert(pElementRemonte, (byte)(pop.idxElt + 1), TailleElement, dataRemontee);
          }
          return InsertOrUpdateResult.Inserted;
        }
        // Allocation d'un nouveau noeud dans la profondeur actuelle
        AlloueNoeud(idxProfondeur, dataRemonteeSuivante);

        // split du noeud
        // Choix de l'élément pivot, qui va remonter dans le noeud parent
        int idxInsertion = pop.idxElt + 1;
        if (idxInsertion <= IdxPivot)
        {
          // eltRemonte = noeud[idxPivot] ' copie de l’élément qui va remonter dans le parent
          Array.Copy(pop.noeud.data, 1 + TailleElement * IdxPivot, dataRemonteeSuivante.elementRemonte, 0, TailleElement);
          // Copie de noeud[idxPivot + 1 .. Ordre – 2] --> nouveauNoeud[0 .. Ordre – 3 – idxPivot] ' nb = Ordre – 2 – idxPivot
          Array.Copy(pop.noeud.data, 1 + TailleElement * (IdxPivot + 1), dataRemonteeSuivante.enfantPlus.data, 1, TailleElement * (Ordre - 2 - IdxPivot));
          if (!pop.noeud.IsFeuille)
          {
            // copie de noeud.Enfants[idxPivot + 1 .. Ordre – 1] --> nouveauNoeud.Enfants[0 .. Ordre – 2 – idxPivot] ' nb = Ordre – 1 – idxPivot
            pop.noeud.CopyEnfants(IdxPivot + 1, dataRemonteeSuivante.enfantPlus, 0, Ordre - 1 - IdxPivot);
          }
          // Déplacement de 1 case à droite de noeud[idxInsertion .. idxPivot – 1] 
          Array.Copy(pop.noeud.data, 1 + TailleElement * idxInsertion, pop.noeud.data, 1 + TailleElement * (idxInsertion + 1), TailleElement * (IdxPivot - idxInsertion));
          if (!pop.noeud.IsFeuille)
          {
            // déplacement de 1 case à droite de noeud.Enfants[idxInsertion + 1 .. idxPivot]
            pop.noeud.CopyEnfants(idxInsertion + 1, pop.noeud, idxInsertion + 2, IdxPivot - idxInsertion);
          }
          // noeud[idxInsertion] = eltAInserer
          Array.Copy(dataRemontee.elementRemonte, 0, pop.noeud.data, 1 + TailleElement * idxInsertion, TailleElement);
          if (!pop.noeud.IsFeuille)
          {
            // noeud.Enfants[idxInsertion + 1] = noeudSuperieur
            pop.noeud.SetEnfant(idxInsertion + 1, dataRemontee);
          }
        }
        else if (idxInsertion == IdxPivot + 1)
        {
          // eltRemonte = eltAInserer
          Array.Copy(dataRemontee.elementRemonte, 0, dataRemonteeSuivante.elementRemonte, 0, TailleElement);
          // copie de noeud[idxPivot + 1 .. Ordre – 2] --> nouveauNoeud[0 .. Ordre – 3 – idxPivot] ' nb = Ordre – 2 – idxPivot
          Array.Copy(pop.noeud.data, 1 + TailleElement * (IdxPivot + 1), dataRemonteeSuivante.enfantPlus.data, 1, TailleElement * (Ordre - 2 - IdxPivot));
          if (!pop.noeud.IsFeuille)
          {
            // nouveauNoeud[0] = noeudSuperieur
            dataRemonteeSuivante.enfantPlus.SetEnfant(0, dataRemontee);
            // copie de noeud.Enfants[idxPivot + 2 .. Ordre – 1] --> nouveauNoeud.Enfants[1 .. Ordre – 2 – idxPivot]
            pop.noeud.CopyEnfants(IdxPivot + 2, dataRemonteeSuivante.enfantPlus, 1, Ordre - 2 - IdxPivot);
          }
        }
        else
        {
          // eltRemonte = noeud[idxPivot + 1]
          Array.Copy(pop.noeud.data, 1 + TailleElement * (IdxPivot + 1), dataRemonteeSuivante.elementRemonte, 0, TailleElement);
          // copie de noeud[idxPivot + 2 .. idxInsertion – 1] --> nouveauNoeud[0 .. idxInsertion – 3 – idxPivot] ' nb = idxInsertion – 2 – idxPivot
          Array.Copy(pop.noeud.data, 1 + TailleElement * (IdxPivot + 2), dataRemonteeSuivante.enfantPlus.data, 1, TailleElement * (idxInsertion - 2 - IdxPivot));
          if (!pop.noeud.IsFeuille)
          {
            // copie de noeud.Enfants[idxPivot + 2 .. idxInsertion]  nouveauNoeud[0 .. idxInsertion – 2 – idxPivot]
            pop.noeud.CopyEnfants(IdxPivot + 2, dataRemonteeSuivante.enfantPlus, 0, idxInsertion - 1 - IdxPivot);
          }
          // nouveauNoeud[idxInsertion – 2 – idxPivot] = eltAInserer
          Array.Copy(dataRemontee.elementRemonte, 0, dataRemonteeSuivante.enfantPlus.data, 1 + TailleElement * (idxInsertion - 2 - IdxPivot), TailleElement);
          if (!pop.noeud.IsFeuille)
          {
            // nouveauNoeud.Enfants[idxInsertion – 1 – idxPivot] = noeudSuperieur
            dataRemonteeSuivante.enfantPlus.SetEnfant(idxInsertion - 1 - IdxPivot, dataRemontee);
          }
          // copie de noeud[idxInsertion .. Ordre − 2] --> nouveauNoeud[idxInsertion – 1 − idxPivot .. Ordre – 3 – idxPivot] ' nb = Ordre – 1 – idxInsertion
          Array.Copy(pop.noeud.data, 1 + TailleElement * (idxInsertion), dataRemonteeSuivante.enfantPlus.data, 1 + TailleElement * (idxInsertion - 1 - IdxPivot), TailleElement * (Ordre - 1 - idxInsertion));
          if (!pop.noeud.IsFeuille)
          {
            // copie de noeud.Enfants[idxInsertion + 1 .. Ordre − 1] --> nouveauNoeud[idxInsertion – idxPivot .. Ordre – 2 − idxPivot]
            pop.noeud.CopyEnfants(idxInsertion + 1, dataRemonteeSuivante.enfantPlus, idxInsertion - IdxPivot, Ordre - 1 - idxInsertion);
          }
        }
        // noeud.NbElt = idxPivot + 1
        pop.noeud.NbElements = (byte)(IdxPivot + 1);
        // nouveauNoeud.NbElt = Ordre – 2 – idxPivot
        dataRemonteeSuivante.enfantPlus.NbElements = (byte)(Ordre - 2 - IdxPivot);
        SwitchData();
        pop.noeud.dirty = true;
      }
      // On n'a rencontré aucun noeud non saturé dans lequel insérer l'élément à remonter.
      // On crée alors une nouvelle racine avec ce seul élément, qui pointe sur l'ancienne racine et l'enfant supérieur de l'élément à remonter.
      CreeNouvelleRacine(dataRemontee);
      return InsertOrUpdateResult.Inserted;
    }

    internal virtual void SwitchData()
    {
      DataRemonteeVolatile tmpDataRemontee = dataRemontee;
      // eltAInserer = eltRemonte
      dataRemontee = dataRemonteeSuivante;
      // noeudSuperieur = nouveauNoeud
      dataRemonteeSuivante = tmpDataRemontee;
    }

    // Recherche dichotomique d'un élément E dans une liste triée L de taille T
    // retourne false si E absent de L, true si E trouvé dans L
    // Si E trouvé dans L, idxPosition contient l'indice (0-based) de E dans L
    // Si E absent de L, idxPosition contient l'une des valeurs suivantes :
    //   si E < L[0]
    //     idxPosition vaut -1
    //   sinon
    //     idxPosition est l'indice du plus grand élément E' de L tel que E' < E
    internal unsafe bool RechercheDichotomique(byte* pSituationNew, NoeudVolatile actuel, out int idxPosition)
    {
      int idxInf = 0, idxSup = actuel.NbElements - 1;
      fixed (byte* pSituationData = &actuel.data[1])
      {
        for (; ; )
        {
          int idxPivot = (idxInf + idxSup) / 2;
          byte* pSituationPivot = pSituationData + idxPivot * TailleElement;
          int cmpResult = Comparateur.CompareSituations(pSituationNew, pSituationPivot);
          if (cmpResult == 0)
          {
            idxPosition = idxPivot;
            return true;
          }
          if (cmpResult < 0)
          {
            if (idxPivot == idxInf)
            {
              idxPosition = idxInf - 1;
              return false;
            }
            idxSup = idxPivot - 1;
          }
          else
          {
            if (idxPivot == idxSup)
            {
              idxPosition = idxSup;
              return false;
            }
            idxInf = idxPivot + 1;
          }
        }
      }
    }

    internal unsafe bool RechercheDichotomique(byte[] situation, NoeudVolatile noeud, out int idxPosition)
    {
      fixed (byte* pSituation = situation)
      {
        return RechercheDichotomique(pSituation, noeud, out idxPosition);
      }
    }

    public IEnumerable<byte[]> EnumereElements(byte[] elementDepart = null)
    {
      // Cas particulier de l'arbre vide
      if (Racine.NbElements == 0)
      {
        yield break; // ou simplement return;
      }
      // Initialisation des structures permettant le parcours de l'arbre
      List<DataParcourtVolatile> pile = new List<DataParcourtVolatile>();
      int idxProfondeur, idxElt;
      NoeudVolatile noeud = Racine;
      if (elementDepart == null)
      {
        pile.Add(new DataParcourtVolatile(noeud));
        for (idxProfondeur = Profondeur; idxProfondeur > 0; idxProfondeur--)
        {
          noeud = GetNoeudEnfant(noeud, idxProfondeur, 0);
          pile.Add(new DataParcourtVolatile(noeud));
        }
        // A partir d'ici, idxProfondeur est l'inverse de la "profondeur" utilisée par ailleurs.
        // On pointe le dernier noeud de Pile, qui est une feuille
        idxProfondeur = Profondeur;
      }
      else
      {
        for (idxProfondeur = Profondeur; ; idxProfondeur--)
        {
          if (RechercheDichotomique(elementDepart, noeud, out idxElt))
          {
            pile.Add(new DataParcourtVolatile(noeud, idxElt));
            idxProfondeur = pile.Count - 1;
            break;
          }
          if (noeud.IsFeuille)
          {
            throw new ApplicationException("Impossible de débuter l'itération du contenu de l'arbre à partir d'un élément absent");
          }
          pile.Add(new DataParcourtVolatile(noeud, idxElt + 1));
          noeud = GetNoeudEnfant(noeud, idxProfondeur, idxElt + 1);
        }
        while (pile.Count <= Profondeur)
        {
          pile.Add(new DataParcourtVolatile(null, 0));
        }
      }
      // buffer permettant le renvoi de l'élément à chacun des appels de cet itérateur
      byte[] result = new byte[TailleElement];

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
            for (idxProfondeur--; idxProfondeur >= 0; idxProfondeur--)
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

    internal virtual NoeudVolatile GetNoeudEnfant(NoeudVolatile noeud, int profondeurNoeud, int idxEnfant)
    {
      return noeud.Enfants[idxEnfant];
    }
    
    public unsafe bool Existe(byte* pSituation)
    {
      // Cas particulier de l'arbre vide
      if (Racine.NbElements == 0)
      {
        return false;
      }
      int idxProfondeur;
      NoeudVolatile noeudActuel = Racine;
      for (idxProfondeur = Profondeur; idxProfondeur >= 0; idxProfondeur--)
      {
        bool bFound = RechercheDichotomique(pSituation, noeudActuel, out int idxPosition);
        if (bFound)
        {
          // La situation en paramètre est déjà présente dans l'arbre. idxPosition est alors >= 0
          fixed (byte* pSituationExistante = &noeudActuel.data[1 + idxPosition * TailleElement])
          {
            // On donne une chance à l'appelant de modifier les données variables de la situation stockée dans l'arbre
            // ATTENTION : dans l'insertion,
            //  param1 : situation passée en paramètre
            //  param2 : situation existante dans le B-Tree, et MajSituation renvoie true s'il modifie param2,
            //  et la modification de la situation du B-Tree sera alors enregistrée dans le B-Tree
            // Dans la recherche, 
            //  param1 : situation existante dans le B-Tree, et MajSituation renvoie true s'il modifie param2
            //  param2 : situation passée en paramètre, et MajSituation va mettre à jour la situation passée en paramètre, pas celle du B-Tree
            Comparateur.MajSituation(pSituationExistante, pSituation);
            return true;
          }
        }
        if (idxProfondeur > 0)
        {
          noeudActuel = GetNoeudEnfant(noeudActuel, idxProfondeur, idxPosition + 1);
        }
      }
      // Si on arrive ici, c'est qu'on n'a pas trouvé la situation passée en paramètre
      return false;
    }

    public unsafe bool Existe(byte[] situation)
    {
      fixed(byte*pSituation = situation)
      {
        return Existe(pSituation);
      }
    }
    internal virtual void CreeNouvelleRacine(DataRemonteeVolatile dataRemontee)
    {
      int nouvelleProfondeur = Profondeur + 1;
      NoeudVolatile nouvelleRacine = new NoeudVolatile(Ordre, TailleElement, false);
      Array.Copy(dataRemontee.elementRemonte, 0, nouvelleRacine.data, 1, TailleElement);
      nouvelleRacine.Enfants[0] = Racine;
      nouvelleRacine.Enfants[1] = dataRemontee.enfantPlus;
      nouvelleRacine.NbElements = 1;
      _Profondeur++;
      Racine = nouvelleRacine;
      PileInsertion.Add(new DataParcourtVolatile());
    }
    internal virtual void AlloueNoeud(int idxProfondeur, DataRemonteeVolatile data)
    {
      data.enfantPlus = new NoeudVolatile(Ordre, TailleElement, idxProfondeur == 0);
    }
    // Pour débug, sur des arbres de tailles "modestes"
    public string Dump()
    {
      StringBuilder sb = new StringBuilder();
      Dump(Racine, Profondeur, sb);
      return sb.ToString();
    }
    private void Dump(NoeudVolatile noeud, int profondeur, StringBuilder sb)
    {
      if (!noeud.IsFeuille)
      {
        NoeudVolatile noeudEnfant = GetNoeudEnfant(noeud, profondeur, 0);
        Dump(noeudEnfant, profondeur - 1, sb);
      }
      for (int idxElt = 0; idxElt < noeud.NbElements; idxElt++)
      {
        Dump(noeud.data, idxElt, TailleElement, profondeur, sb);
        if (!noeud.IsFeuille)
        {
          NoeudVolatile noeudEnfant = GetNoeudEnfant(noeud, profondeur, idxElt + 1);
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

    private unsafe void Verifie(NoeudVolatile noeud, int profondeur, ref NoeudVolatile noeudPrecedent, ref int idxElementPrecedent)
    {
      if (!noeud.IsFeuille)
      {
        NoeudVolatile noeudEnfant = GetNoeudEnfant(noeud, profondeur, 0);
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
          NoeudVolatile noeudEnfant = GetNoeudEnfant(noeud, profondeur, idxElt + 1);
          Verifie(noeudEnfant, profondeur - 1, ref noeudPrecedent, ref idxElementPrecedent);
        }
      }
    }

    internal unsafe string Dump(NoeudPersistant noeud)
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

    private void CollecteStats(List<StatsProfondeur> stats, int idxProfondeur, NoeudVolatile noeud)
    {
      StatsProfondeur s = stats[idxProfondeur];
      s.nbNoeuds++;
      s.nbElements += noeud.NbElements;
      if (!noeud.IsFeuille)
      {
        CollecteStats(stats, idxProfondeur - 1, GetNoeudEnfant(noeud, idxProfondeur, 0));
        for (int idxElt = 0; idxElt < noeud.NbElements; idxElt++)
        {
          CollecteStats(stats, idxProfondeur - 1, GetNoeudEnfant(noeud, idxProfondeur, idxElt + 1));
        }
      }
    }
  }
}
