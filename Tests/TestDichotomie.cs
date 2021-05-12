using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Tests
{

  [TestClass]
  public class TestDichotomie
  {
    // Test de la méthode de recherche dichotomique.
    [TestMethod]
    public void Test()
    {
      Random rnd = new Random();
      int nbMaxValeursInTableau = 100;
      for (int nbValeursInTableau = 1; nbValeursInTableau <= nbMaxValeursInTableau; nbValeursInTableau++)
      {
        // Préparation du tableau trié des valeurs uniques
        int nbValeursAutorisees = 10 * nbValeursInTableau;
        // Liste des valeursAutorisees autorisées de 0 à nbValeursAutorisees-1
        List<int> valeursAutorisees = new List<int>();
        for (int idx = 0; idx < nbValeursAutorisees; idx++)
        {
          valeursAutorisees.Add(idx);
        }
        int[] tableau = new int[nbValeursInTableau];
        for (int idx = 0; idx < nbValeursInTableau; idx++)
        {
          // Choix d'une valeur autorisée, et inscription dans le tableau
          int idxValeur = rnd.Next(valeursAutorisees.Count);
          tableau[idx] = valeursAutorisees[idxValeur];
          // Pour être certain que les valeursAutorisees soient uniques, on supprime de la liste des valeurs autorisées celle qu'on vient d'ajouter dans le tableau
          valeursAutorisees.RemoveAt(idxValeur);
        }
        // Tri du tableau des valeurs
        Array.Sort(tableau);
        // Vérification que le tableau est trié et que ses valeurs sont uniques.
        for (int idx = 1; idx < nbValeursInTableau; idx++)
        {
          Assert.IsTrue(tableau[idx - 1] < tableau[idx]);
        }
        // Test de chacune des valeurs possibles, depuis -1 à max(valeur autorisée)+1
        for (int valeurTestee = -1; valeurTestee <= nbValeursAutorisees; valeurTestee++)
        {
          // Recherche d'une valeur
          bool found = RechercheDichotomique(tableau, valeurTestee, out int idxPos);
          if (found)
          {
            Assert.IsTrue(idxPos >= 0);
            Assert.IsTrue(idxPos < nbValeursInTableau);
            Assert.AreEqual(valeurTestee, tableau[idxPos]);
          }
          else if (idxPos < 0)
          {
            Assert.IsTrue(idxPos == -1);
            Assert.IsTrue(valeurTestee < tableau[0]);
          }
          else
          {
            Assert.IsTrue(idxPos < nbValeursInTableau);
            Assert.IsTrue(tableau[idxPos] < valeurTestee);
            Assert.IsTrue(idxPos == nbValeursInTableau - 1 || valeurTestee < tableau[idxPos + 1]);
          }
        }
      }
    }

    // Recherche dichotomique d'un élément E dans une liste triée L de taille T
    // retourne false si E absent de L, true si E trouvé dans L
    // Si E trouvé dans L, idxPosition contient l'indice (0-based) de E dans L
    // Si E absent de L, idxPosition contient l'une des valeurs suivantes :
    //   si E < L[0]
    //     -1
    //   sinon
    //     l'indice du plus grand élément E' de L tel que E' < E
    private bool RechercheDichotomique(int[] tableau, int valeurTestee, out int idxPosition)
    {
      int idxInf = 0, idxSup = tableau.Length - 1;
      for (; ; )
      {
        int idxPivot = (idxInf + idxSup) / 2;
        int cmpResult = valeurTestee.CompareTo(tableau[idxPivot]);
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
            idxPosition = idxPivot;
            return false;
          }
          idxInf = idxPivot + 1;
        }
      }
    }
  }
}
