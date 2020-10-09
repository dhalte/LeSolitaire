using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class Tests
  {

    // Une collision se produit lorsque deux situations différentes ont même hashcode.
    // On parcourt donc les situations à 14 pierres de EF.dat et on décompte le nombre
    // de situations réparties selon leur clé de hashage.
    // Résultat :
    // nombre de clés, nombre de situations, % delta, nb int.MaxValue
    //     74 329 954,           75 432 581, 1.4617 %,              0
    // nb situations par hashcode, nb hashcodes ayant ce nb de situations, % delta
    //                          1,                             73 239 255, 97.0923 %
    //                          2,                              1 078 868,  1.4302 %
    //                          3,                                 11 734,  0.0156 %
    //                          4,                                     97,  0.0002 %
    public static void EvaluePertinenceHashage()
    {
      string fileName = @"C:\Users\halte\reposDivers\LeSolitaire\Jeux\Plateau Français\EF.dat";
      // il y a 1 pierre dans une SG, 13 mouvements qui séparent une SF d'une SG, donc 14 pierres dans une SF.
      int NbPierres = 14;
      byte[] buffer = new byte[NbPierres];
      Dictionary<int, int> stats = new Dictionary<int, int>();
      int nbSituations = 0;
      int nbSituationsMaxPourUnHashcode = 0;
      int nIntMaxEncountered = 0;
      using (FileStream EFdat = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      {
        long EFlen = EFdat.Length;
        Debug.Assert(EFlen % NbPierres == 0);
        for (; ; )
        {
          int n = EFdat.Read(buffer, 0, NbPierres);
          if (n == 0)
          {
            break;
          }
          Debug.Assert(n == NbPierres);
          nbSituations++;
          Situation situation = new Situation(buffer);
          int hashcode = situation.GetHashCode();
          if (hashcode == int.MaxValue)
          {
            nIntMaxEncountered++;
          }
          if (!stats.ContainsKey(hashcode))
          {
            stats.Add(hashcode, 0);
          }
          int nbSituationsPourCeHashcode = stats[hashcode] + 1;
          stats[hashcode] = nbSituationsPourCeHashcode;
          if (nbSituationsMaxPourUnHashcode < nbSituationsPourCeHashcode)
          {
            nbSituationsMaxPourUnHashcode = nbSituationsPourCeHashcode;
          }
        }
      }
      Debug.Print($"nombre de clés, nombre de situations, % delta, nb int.MaxValue");
      Debug.Print($"{stats.Keys.Count}, {nbSituations}, {100f * (nbSituations - stats.Keys.Count) / nbSituations}, {nIntMaxEncountered}");
      int[] repartition = new int[nbSituationsMaxPourUnHashcode];
      foreach (int nbSituationsPourCeHashcode in stats.Values)
      {
        repartition[nbSituationsPourCeHashcode - 1]++;
      }
      Debug.Print("nb situations par hashcode, nb hashcodes ayant ce nb de situations, % delta");
      for (int idxNb = 0; idxNb < nbSituationsMaxPourUnHashcode; idxNb++)
      {
        Debug.Print($"{idxNb + 1}, {repartition[idxNb]}, {100f * (repartition[idxNb]) / nbSituations}");
      }
    }


    private class CompareSituations : IComparer<byte[]>
    {
      public int Compare(byte[] x, byte[] y)
      {
        int l = x.Length;
        Debug.Assert(l == y.Length);
        for (int i = 0; i < l; i++)
        {
          if (x[i] != y[i]) return x[i].CompareTo(y[i]);
        }
        return 0;
      }
    }

    // Encore une fois, impossible de faire exécuter ce test dans un projet de type UnitTest, il déclenche des OutOfMemory Exceptions.
    // Chargement du contenu de EF.dat ancien format dans un SortedSet
    // nf==13, nbPierres==14, nombre de SF : 75 432 581, taille de EF.dat : 1 056 056 134 octets
    // Chargement en moins de 3 minutes quand on ne cherche pas pour chaque situation la symétrique "minimale".
    // Chargement en moins de 8 minutes quand on cherche pour chaque situation sa situation symétrique "minimale".
    // Sauvegarde en moins de 12.30 secondes
    public static void BuildBTree()
    {
      string descriptionPlateau = @"
  xxx
 xxxxx
xxxxxxx
xxxxxxx
xxxxxxx
 xxxxx
  xxx
";
      string EFfileName = @"C:\Users\halte\reposDivers\LeSolitaire\Jeux\Plateau Français\EF.dat";
      string EFsortedfileName = @"C:\Users\halte\reposDivers\LeSolitaire\Jeux\Plateau Français\EF.sorted";
      Plateau plateau = new Plateau(Common.ChargeSituationRaw(descriptionPlateau));
      // il y a 1 pierre dans une SG, 13 mouvements qui séparent une SF d'une SG, donc 14 pierres dans une SF.
      int NbPierres = 14;
      SituationEtude situationEtude = new SituationEtude(plateau);
      // Va recueillir et trier 75 432 581 de byte[14]
      SortedSet<byte[]> sortedSet = new SortedSet<byte[]>(new CompareSituations());
      // buffers recueillant les symétries pour comparaisons
      byte[][] buffers;
      buffers = new byte[plateau.NbSymetries][];
      for (int idxSymetrie = 0; idxSymetrie < plateau.NbSymetries; idxSymetrie++)
      {
        buffers[idxSymetrie] = new byte[NbPierres];
      }
      Stopwatch stopwatch = Stopwatch.StartNew();
      using (FileStream EFdat = new FileStream(EFfileName, FileMode.Open, FileAccess.Read))
      {
        long EFlen = EFdat.Length;
        Debug.Assert(EFlen % NbPierres == 0);
        CompareSituations compareSituations = new CompareSituations();
        for (; ; )
        {
          int n = EFdat.Read(buffers[0], 0, NbPierres);
          if (n == 0)
          {
            break;
          }
          Debug.Assert(n == NbPierres);
          plateau.GenereSymetries(buffers);
          for (int idxSymetrie = 0; idxSymetrie < plateau.NbSymetries; idxSymetrie++)
          {
            byte[] idxPierres = buffers[idxSymetrie];
            // Debug.Print($"{plateau.Dump(idxPierres)}");
          }
          int idxBest = 0;
          for (int idxBest1 = 0; idxBest1 < plateau.NbSymetries; idxBest1++)
          {
            if (compareSituations.Compare(buffers[idxBest], buffers[idxBest1]) > 0)
            {
              idxBest = idxBest1;
            }
          }
          byte[] situation = new byte[NbPierres];
          Array.Copy(buffers[idxBest], situation, NbPierres);
          sortedSet.Add(situation);
        }
      }
      stopwatch.Stop();
      Debug.Print($"temps de chargement dans le SortedSet : {stopwatch.Elapsed}");
      stopwatch.Restart();
      using (FileStream EFdat = new FileStream(EFsortedfileName, FileMode.Create, FileAccess.Write))
      {
        foreach (byte[] buffer in sortedSet)
        {
          EFdat.Write(buffer, 0, NbPierres);
        }
      }
      Debug.Print($"temps de sauvegarde du SortedSet : {stopwatch.Elapsed}");
    }

    // Lecture aléatoire de 10 000 entrées de EF.order : < 0.08 sec
    // Recherche dichotomique de ces 10 000 entrées    : < 1.25 sec
    public static void TesteRechercheSituationInEfsorted()
    {
      string EFsortedfileName = @"C:\Users\halte\reposDivers\LeSolitaire\Jeux\Plateau Français\EF.sorted";
      // il y a 1 pierre dans une SG, 13 mouvements qui séparent une SF d'une SG, donc 14 pierres dans une SF.
      int NbPierres = 14;
      int nbTests = 10_000;
      byte[][] situationsTestees = new byte[nbTests][];
      for (int idxTest = 0; idxTest < nbTests; idxTest++)
      {
        situationsTestees[idxTest] = new byte[NbPierres];
      }
      Stopwatch stopwatch = Stopwatch.StartNew();
      using (FileStream EFdat = new FileStream(EFsortedfileName, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        long EFlen = EFdat.Length;
        Debug.Assert(EFlen % NbPierres == 0);
        int nbSituations = (int)(EFlen / NbPierres);
        Random random = new Random();
        for (int idxTest = 0; idxTest < nbTests; idxTest++)
        {
          int idxSituationTestee = random.Next(0, nbSituations - 1);
          EFdat.Seek(idxSituationTestee * NbPierres, SeekOrigin.Begin);
          EFdat.Read(situationsTestees[idxTest], 0, NbPierres);
        }
      }
      Debug.Print($"Chargement des {nbTests} en {stopwatch.Elapsed}");
      stopwatch.Restart();
      CompareSituations compareSituations = new CompareSituations();
      using (FileStream EFdat = new FileStream(EFsortedfileName, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        long EFlen = EFdat.Length;
        Debug.Assert(EFlen % NbPierres == 0);
        int nbSituations = (int)(EFlen / NbPierres);
        for (int idxTest = 0; idxTest < nbTests; idxTest++)
        {
          byte[] buffer = new byte[NbPierres];
          int idxMin = 0, idxMax = nbSituations - 1;
          int idxProbe;
          for (; ; )
          {
            idxProbe = (idxMin + idxMax) / 2;
            EFdat.Seek(idxProbe * NbPierres, SeekOrigin.Begin);
            EFdat.Read(buffer, 0, NbPierres);
            int nCmp = compareSituations.Compare(situationsTestees[idxTest], buffer);
            if (nCmp == 0)
            {
              break;
            }
            if (nCmp < 0)
            {
              idxMax = idxProbe - 1;
            }
            else
            {
              idxMin = idxProbe + 1;
            }
            if (idxMin > idxMax)
            {
              Debug.Print("Impossible de retrouver la situation");
            }
          }
        }
      }

      Debug.Print($"Recherche des {nbTests} en {stopwatch.Elapsed}");

    }
  }
}