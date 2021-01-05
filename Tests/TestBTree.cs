using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Tests
{
  /// <summary>
  /// Description résumée pour TestBTree
  /// </summary>
  [TestClass]
  public class TestBTree
  {
    public TestBTree()
    {
      //
      // TODO: ajoutez ici la logique du constructeur
      //
    }

    private TestContext testContextInstance;

    /// <summary>
    ///Obtient ou définit le contexte de test qui fournit
    ///des informations sur la série de tests active, ainsi que ses fonctionnalités.
    ///</summary>
    public TestContext TestContext
    {
      get
      {
        return testContextInstance;
      }
      set
      {
        testContextInstance = value;
      }
    }

    #region Attributs de tests supplémentaires
    //
    // Vous pouvez utiliser les attributs supplémentaires suivants lorsque vous écrivez vos tests :
    //
    // Utilisez ClassInitialize pour exécuter du code avant d'exécuter le premier test de la classe
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
    //
    // Utilisez ClassCleanup pour exécuter du code une fois que tous les tests d'une classe ont été exécutés
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    // Utilisez TestInitialize pour exécuter du code avant d'exécuter chaque test 
    // [TestInitialize()]
    // public void MyTestInitialize() { }
    //
    // Utilisez TestCleanup pour exécuter du code après que chaque test a été exécuté
    // [TestCleanup()]
    // public void MyTestCleanup() { }
    //
    #endregion

    // On crée un tableau de Width entiers remplis aléatoirement entre les bornes Bmin et Bmax puis triés
    // Puis on teste la méthode de recherche dichotomique pour chaque entier entre bMin et bMax
    // Et on vérifie le résultat obtenu
    [TestMethod]
    public void TestRechercheDichotomique()
    {
      #region Préparation données
      Random random = new Random();
      // On fait varier le nombre de situations dans le noeud entre 1 et 32.
      // Le cas de la racine vide doit être traité séparément.
      int nbTests = 100_000;
      // Les indices des pierres varient de 0 à 48
      int nbCases = 49; // Doit être >= à Width
      for (int idxTest = 0; idxTest < nbTests; idxTest++)
      {
        int Width = random.Next(1, 33);
        List<int> idxPierres = new List<int>();
        for (int idxPierre = 0; idxPierre < nbCases; idxPierre++)
        {
          idxPierres.Add(idxPierre);
        }
        int[] tbl = new int[Width];
        for (int idxBuffer = 0; idxBuffer < Width; idxBuffer++)
        {
          int i = random.Next(idxPierres.Count);
          tbl[idxBuffer] = idxPierres[i];
          idxPierres.RemoveAt(i);
        }
        Array.Sort(tbl);
        #endregion Préparation données
        // test proprement dit
        for (int v = 0; v < nbCases; v++)
        {
          // rappel : ne supporte pas les tableaux vides
          RechercheDichotomique(tbl, v, out int idx, out bool present);
          if (present)
          {
            Assert.IsTrue(0 <= idx);
            Assert.IsTrue(idx < tbl.Length);
            Assert.IsTrue(tbl[idx] == v);
          }
          else
          {
            if (idx == -1)
            {
              Assert.IsTrue(v < tbl[0]);
            }
            else if (tbl.Length - 1 == idx)
            {
              Assert.IsTrue(tbl[tbl.Length - 1] < v);
            }
            else
            {
              Assert.IsTrue(0 <= idx);
              Assert.IsTrue(idx < tbl.Length);
              Assert.IsTrue(tbl[idx] < v);
              Assert.IsTrue(v < tbl[idx + 1]);
            }
          }
        }
      }
    }

    private void RechercheDichotomique(int[] tbl, int v, out int idx, out bool present)
    {
      int borneMin = 0, borneMax = tbl.Length - 1, milieu;
      for (; ; )
      {
        milieu = (borneMin + borneMax) / 2;
        int n = v.CompareTo(tbl[milieu]);
        if (n < 0)
        {
          if (borneMin == milieu)
          {
            idx = borneMin - 1;
            present = false;
            return;
          }
          borneMax = milieu - 1;
        }
        else if (0 < n)
        {
          if (milieu == borneMax)
          {
            idx = borneMax;
            present = false;
            return;
          }
          borneMin = milieu + 1;
        }
        else
        {
          idx = milieu;
          present = true;
          return;
        }
      }
    }

    // Simulation d'un noeud du B-Tree
    private class Noeud
    {
      // Pour le test, on se contente de simuler une situation par un simple entier
      // Et on ne stocke pas les offsets des enfants sur disque
      public int nbSituationsActuelles;
      public int[] Situations;

      public Noeud(int ordre)
      {
        // ordre est le nombre d'enfants, c'est 1+le nombre de clés
        Situations = new int[ordre - 1];
      }
      // Pour l'instant on ne s'en occupe pas
      //      public Noeud[] Enfants;
    }
    // On va remplir un noeud avec des situations
    [TestMethod]
    public void TestInsertion()
    {
      Random random = new Random();
      int nbTests = 100_000;
      int ordre;
      for (int idxTest = 0; idxTest < nbTests; idxTest++)
      {
        ordre = 5 + idxTest % 40;
        // situation est la situation à tester et à insérer si non trouvée
        for (int situation = 0; situation <= 2 * ordre; situation++)
        {
          Noeud noeud = new Noeud(ordre);
          // On simule un noeud complet, On initialise les situations à 2 4 6 ...
          noeud.nbSituationsActuelles = ordre - 1;
          for (int i = 0; i < noeud.Situations.Length; i++)
          {
            noeud.Situations[i] = 2 * (i + 1);
          }
          // noeud.Enfants = new Noeud[ordre]; // ils restent à null, on est dans une feuille
          RechercheSituation(noeud, situation, out int idx, out bool present);
          int[] tbl = noeud.Situations;
          if (present)
          {
            Assert.IsTrue(0 <= idx);
            Assert.IsTrue(idx < tbl.Length);
            Assert.IsTrue(tbl[idx] == situation);
          }
          else
          {
            InsereSituationSplitNode(noeud, situation, idx, out int situationPivot, out Noeud enfant);
            // Vérifications que l'insertion est consistante
            int nbCles = 1; // on ne vérifie pas la 1ère clé de l'ancien noeud
            for (int i = 1; i < noeud.nbSituationsActuelles; i++)
            {
              Assert.IsTrue(noeud.Situations[i - 1] < noeud.Situations[i]);
              nbCles++;
            }
            Assert.IsTrue(noeud.Situations[noeud.nbSituationsActuelles - 1] < situationPivot);
            nbCles++;
            Assert.IsTrue(situationPivot < enfant.Situations[0]);
            nbCles++;
            for (int i = 1; i < enfant.nbSituationsActuelles; i++)
            {
              Assert.IsTrue(enfant.Situations[i - 1] < enfant.Situations[i]);
              nbCles++;
            }
            // Au début, on a ordre-1 clés, donc ordre clés après insertion
            Assert.IsTrue(nbCles == ordre);
          }
        }
      }
    }

    private void RechercheSituation(Noeud noeud, int situation, out int index, out bool present)
    {
      RechercheDichotomique(noeud.Situations, situation, out index, out present);
    }

    // On doit insérer la situation dans le noeud déjà complet
    // idx est la position de la situation dans le noeud juste inférieure à celle qu'on doit insérer
    // ou -1 si la situation à insérer est < à celles du noeud
    // ou noeud.situations.Length si la situation à insérer est > à celles du noeud
    // On va donc créer un nouveau noeud, y placer les situations supérieures, 
    // laisser dans le noeud courant les situations inférieures
    // et extraire la situation médiane, destinée à être insérée dans le noeud parent
    private void InsereSituationSplitNode(Noeud noeud, int situation, int idx, out int situationPivot, out Noeud enfant)
    {

      // ordre : nombre max d'enfants, c'est 1+le nombre max de situations.
      // n = ordre-1 : nombre maximum de clés, indicées de 0 à n-1
      // iPivot = (n-1)/2 : indice du pivot pressenti
      int ordre = noeud.Situations.Length + 1;
      int n = ordre - 1;
      int iPivot = (n - 1) / 2; // division euclidienne
      enfant = new Noeud(ordre);
      if (idx < iPivot)
      {
        //U <-- noeud[iPivot]	
        //newNoeud[0 .. n-2-iPivot] <-- nœud[iPivot+1 .. n-1]	
        //newNoeud.len <-- n-1-iPivot	
        //nœud[iC+2 .. iPivot] <-- nœud[iC+1 .. iPivot-1]	
        //nœud[iC+1] <-- C	
        //nœud.len <-- iPivot+1	
        situationPivot = noeud.Situations[iPivot];
        enfant.nbSituationsActuelles = n - 1 - iPivot;
        Array.Copy(noeud.Situations, iPivot + 1, enfant.Situations, 0, enfant.nbSituationsActuelles);
        Array.Copy(noeud.Situations, idx + 1, noeud.Situations, idx + 2, iPivot - idx - 1);
        noeud.Situations[idx + 1] = situation;
        noeud.nbSituationsActuelles = iPivot + 1;
      }
      else if (idx == iPivot)
      {
        //U <-- C
        //newNoeud[0 .. n-2-iPivot] <-- nœud[iPivot+1..n-1]
        //newNoeud.len <-- n-1-iPivot
        //nœud.len <-- iPivot+1
        situationPivot = situation;
        enfant.nbSituationsActuelles = n - 1 - iPivot;
        Array.Copy(noeud.Situations, iPivot + 1, enfant.Situations, 0, enfant.nbSituationsActuelles);
        noeud.nbSituationsActuelles = iPivot + 1;
      }
      else
      {
        // idx > idxMedian
        //U <-- nœud[iPivot+1]
        //newNoeud[0..iC-iPivot-2] <-- nœud[iPivot+2..iC]
        //newNoeud[iC-iPivot-1] <-- C
        //newNoeud[iC-iPivot .. n-iPivot-2] <-- nœud[iC+1 .. n-1]
        //nœud.len <-- iPivot+1
        //newNoeud.len <-- n - iPivot - 1
        situationPivot = noeud.Situations[iPivot + 1];
        Array.Copy(noeud.Situations, iPivot + 2, enfant.Situations, 0, idx - iPivot - 1);
        enfant.Situations[idx - iPivot - 1] = situation;
        Array.Copy(noeud.Situations, idx + 1, enfant.Situations, idx - iPivot, n - idx - 1);
        noeud.nbSituationsActuelles = iPivot + 1;
        enfant.nbSituationsActuelles = n - iPivot - 1;
      }
    }

  }
}
