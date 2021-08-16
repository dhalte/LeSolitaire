using LeSolitaireLogique.Services;
using BTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LeSolitaireLogique
{
  // Cette classe permet de lancer tous les traitements liés aux opérations de recherche de solution
  public class Moteur
  {
    private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    // Le répertoire contient ou va contenir toutes les informations persistentes liées à la recherche
    private readonly DirectoryInfo Repertoire;
    // L'objet à qui seront communiquées diverses opérations dans les traitements en tâche de fond
    private readonly Feedback Parent;
    // Toutes les informations et opérations relatives au plateau du jeu
    private Plateau Plateau;
    private const int Ordre = 32;
    // On prévoit un emplacement dans la liste pour chaque étape.
    // Lors de la recherche en largeur, seules la dernière étape et la nouvelle sont utilisés
    // Lors de la recherche en profondeur, seule la dernière est utilisée
    // Mais lors de la consolidation d'une solution, il sera utile d'accéder aux autres étapes.
    private List<BTreePersistant> Stockages;
    // Stockage en mémoire uniquement des situations rencontrées lors de la recherche en profondeur
    private EtapesRechercheEnProfondeur Etapes;
    public Moteur(string repertoire, Feedback parent)
    {
      Repertoire = new DirectoryInfo(repertoire);
      Parent = parent;
      SurveillanceMemoire = new SurveillanceMemoire();
      SurveillanceMemoire.LowMemoryEvent += LowMemorySignal;
      LowMemoryState = false;
      DemandeSuspension = false;
      SurveillanceMemoire.Start();
    }
    private bool LowMemoryState;
    private void LowMemorySignal(object sender, EventArgs e)
    {
      LowMemoryState = true;
    }

    private bool DemandeSuspension;

    public unsafe void Initialise(string descriptionPlateau)
    {
      if (Repertoire.Exists && (Repertoire.GetFiles().Length > 0 || Repertoire.GetDirectories().Length > 0))
      {
        throw new ApplicationException("Pour une initialisation, le répertoire doit être vide");
      }
      Plateau = Services.Plateau.DecodeDescription(descriptionPlateau);
      List<byte[]> situationsInitiales = Plateau.CalculeSituationsInitiales();
      if (!Repertoire.Exists)
      {
        // Création du répertoire qui contiendra les autres répertoires de données
        Repertoire.Create();
      }
      Plateau.InitialiseFichierPilote(Repertoire);
      Stockages = new List<BTreePersistant>();
      DirectoryInfo repertoire = new DirectoryInfo(Path.Combine(Repertoire.FullName, "0"));
      BTreePersistant stockage = new BTreePersistant(repertoire, Services.Plateau.TailleDescriptionSituationEtMasque, Ordre, Plateau);
      stockage.InitBTree();
      foreach (byte[] situationInitiale in situationsInitiales)
      {
        fixed (byte* pSituationInitiale = situationInitiale)
        {
          stockage.InsertOrUpdate(pSituationInitiale);
        }
      }
      stockage.Flush();
      stockage.Close();
      Parent.Feedback(FeedbackHint.trace, $"Plateau initialisé :{Environment.NewLine}{Plateau.DescriptionPlateau()}");
    }

    public void LancerVerifier()
    {
      Task taskVerifier = new Task(Verifier);
      taskVerifier.Start();
    }

    public void LancerStats()
    {
      Task taskStats = new Task(BuildStats);
      taskStats.Start();
    }

    public void Suspendre()
    {
      DemandeSuspension = true;
    }

    private SurveillanceMemoire SurveillanceMemoire;
    public void LanceConsolidation()
    {
      Task taskConsolidation = new Task(Consolidation);
      taskConsolidation.Start();
    }

    private void Consolidation()
    {
      try
      {
        Parent.Feedback(FeedbackHint.startOfJob, "Consolidation des solutions partielles");
        InitStockage();

        while (Plateau.GetSolutionPartielle(out Solution solution))
        {
          byte[] situationFinale = new byte[Plateau.TailleDescriptionSituationEtMasque];
          Plateau.ConsoliderSolutionPartielle(solution, situationFinale);
          // Au retour de la fonction précédente, situationFinale[] contient la dernière situation obtenue et les solution.Mouvements ont été convertis.
          // On va maintenant chercher une liste de mouvements qui résoud situationFinale
          List<int> mouvementsFinaux = new List<int>();
          // Pour cela, on part de la situation duale
          byte[] dual = Plateau.CalculeDual(situationFinale);
          int nbPierres = Plateau.NbPierres(dual);
          // On passe en paramètre profondeur l'indice de Stockages[] qui contient éventuellement les situations précédentes qu'on va calculer à partir de dual
          // dual a nbPierres pierres, ce qui correspond à une profondeur de NbCases - 1 - nbPierres
          // C'est pourquoi on passe NbCases - 2 - nbPierres
          int profondeur = Plateau.NbCases - 2 - nbPierres;
          bool b = ConsoliderPartieFinale(dual, mouvementsFinaux, profondeur);
          solution.Mouvements.AddRange(mouvementsFinaux);
          ConsoliderPartieInitiale(solution);
          Plateau.SupprimeSolutionPartielle(solution);
          Plateau.AjouteSolutionsCompletes(solutionsCompletes);
          Plateau.EnregistrerSolutions(Repertoire);
        }
      }
      catch (ApplicationException ex)
      {
        Parent.Feedback(FeedbackHint.error, ex.Message);
      }
      catch (Exception ex)
      {
        Parent.Feedback(FeedbackHint.error, ex.ToString());
      }
      finally
      {
        Parent.Feedback(FeedbackHint.endOfJob, "Arrêt de la tâche de consolidation");
      }
    }

    // Recherche récursive d'une suite de mouvements qui permettent de résoudre ce dual
    // c'est à dire de remonter à une situation initiale, quelle qu'elle soit, à partir du dual.
    private bool ConsoliderPartieFinale(byte[] situation, List<int> mouvements, int profondeur)
    {
      if (profondeur < 0)
      {
        return true;
      }
      byte[] situationPrecedente = new byte[Plateau.TailleDescriptionSituationEtMasque];
      byte[] situationNormalisee = new byte[Plateau.TailleDescriptionSituationEtMasque];
      for (int idxMvt = 0; idxMvt < Plateau.MouvementsPossibles.Count; idxMvt++)
      {
        if (Plateau.MouvementDualAutorise(situation, idxMvt))
        {
          Plateau.MouvementDualEffectue(situation, idxMvt, situationPrecedente);
          Array.Copy(situationPrecedente, situationNormalisee, Plateau.TailleDescriptionSituationEtMasque);
          Plateau.Normalise(situationNormalisee, out _);
          if (Stockages[profondeur].Existe(situationNormalisee))
          {
            if (ConsoliderPartieFinale(situationPrecedente, mouvements, profondeur - 1))
            {
              mouvements.Insert(0, idxMvt);
              return true;
            }
          }
        }
      }
      return false;
    }

    byte fSituationsInitialesResolues;
    byte fSituationsInitialesInitiales;
    List<Solution> solutionsCompletes;
    private void ConsoliderPartieInitiale(Solution solution)
    {
      solutionsCompletes = new List<Solution>();
      fSituationsInitialesResolues = Plateau.CalculeSolutionsCompletesConnues();
      byte[] situationIntermediaire = solution.Situation;
      int nbPierres = Plateau.NbPierres(situationIntermediaire);
      // On passe en paramètre profondeur l'indice de Stockages[] qui contient éventuellement les situations précédentes qu'on va calculer à partir de situationIntermediaire
      // situationIntermediaire a nbPierres pierres, ce qui correspond à une profondeur de NbCases - 1 - nbPierres
      // C'est pourquoi on passe NbCases - 2 - nbPierres
      int profondeur = Plateau.NbCases - 2 - nbPierres;
      fSituationsInitialesInitiales = solution.Situation[Plateau.TailleDescriptionSituationEtMasque - 1];
      int[] mouvements = new int[profondeur + 1];

      ConsoliderPartieInitiale(situationIntermediaire, mouvements, profondeur);
      foreach (Solution sol in solutionsCompletes)
      {
        sol.Mouvements.AddRange(solution.Mouvements);
      }
    }

    private void ConsoliderPartieInitiale(byte[] situation, int[] mouvements, int profondeur)
    {
      byte[] situationNormalisee = new byte[Plateau.TailleDescriptionSituationEtMasque];
      if (profondeur < 0)
      {
        // Récupérer le flag de la situation initiale associée
        Array.Copy(situation, situationNormalisee, Plateau.TailleDescriptionSituationEtMasque);
        Plateau.Normalise(situationNormalisee, out int _);
        situationNormalisee[Plateau.TailleDescriptionSituationEtMasque - 1] = 0;
        bool _ = Stockages[0].Existe(situationNormalisee);
        // Restaurer le flag de situation qui est initiale mais a conservé le flag de la situation de départ de la solution partielle
        situation[Plateau.TailleDescriptionSituationEtMasque - 1] = situationNormalisee[Plateau.TailleDescriptionSituationEtMasque - 1];
        // Cette solution démarre d'une situation initiale non forcément normalisée
        Solution newSol = new Solution(situation, mouvements.ToList());
        solutionsCompletes.Add(newSol);
        fSituationsInitialesResolues |= situationNormalisee[Plateau.TailleDescriptionSituationEtMasque - 1];
        return;
      }

      byte[] situationPrecedente = new byte[Plateau.TailleDescriptionSituationEtMasque];
      for (int idxMvt = 0; idxMvt < Plateau.MouvementsPossibles.Count; idxMvt++)
      {
        if (Plateau.MouvementDualAutorise(situation, idxMvt))
        {
          Plateau.MouvementDualEffectue(situation, idxMvt, situationPrecedente);
          Array.Copy(situationPrecedente, situationNormalisee, Plateau.TailleDescriptionSituationEtMasque);
          Plateau.Normalise(situationNormalisee, out _);
          situationNormalisee[Plateau.TailleDescriptionSituationEtMasque - 1] = 0;
          if (Stockages[profondeur].Existe(situationNormalisee))
          {
            byte fSituationsInitialesSituationPrecedente = situationNormalisee[Plateau.TailleDescriptionSituationEtMasque - 1];
            if ((fSituationsInitialesSituationPrecedente & fSituationsInitialesInitiales & ~fSituationsInitialesResolues) != 0)
            {
              mouvements[profondeur] = idxMvt;
              ConsoliderPartieInitiale(situationPrecedente, mouvements, profondeur - 1);
              if ((fSituationsInitialesResolues & fSituationsInitialesInitiales) == fSituationsInitialesInitiales)
              {
                break;
              }
            }
          }
        }
      }
    }

    public void LanceRechercheEnLargeur()
    {
      Task taskRechercheEnLargeur = new Task(RechercheEnLargeur);
      taskRechercheEnLargeur.Start();
    }

    private void RechercheEnLargeur()
    {
      System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
      int nbSituationsTraitees = 0, nbNoChange = 0, nbUpdated = 0, nbInserted = 0;
      try
      {
        Parent.Feedback(FeedbackHint.startOfJob, "Recherche en largeur");
        InitStockage();
        if (Plateau.Reprise == Services.EnumReprise.None)
        {
          Parent.Feedback(FeedbackHint.info, $"Création du niveau {Stockages.Count}");
          IncrementeStockage();
        }
        else
        {
          Parent.Feedback(FeedbackHint.info, $"Reprise du niveau {Stockages.Count - 1}");
        }
        Parent.Feedback(FeedbackHint.info, $"niveau;nb insérés;durée(ms)");
        BTreePersistant origine = Stockages[Stockages.Count - 2], destination = Stockages[Stockages.Count - 1];
        if (Plateau.Reprise == Services.EnumReprise.None) Plateau.SituationReprise = null;
        string nl = Environment.NewLine;
        string sep = new string('-', 30);
        foreach (byte[] situation in origine.EnumereElements(Plateau.SituationReprise))
        {
          nbSituationsTraitees++;
          if (logger.IsTraceEnabled)
          {
            string flag = situation[Services.Plateau.TailleDescriptionSituationEtMasque - 1].ToString("X").PadLeft(2, '0');
            logger.Trace($"{nl}{sep}{nl}Traitement de{nl}{Plateau.Dump(situation)}Flag:{flag}");
          }
          Plateau.EnumerationNouvellesSituationsNormalisees x = new Plateau.EnumerationNouvellesSituationsNormalisees(Plateau, situation);
          while (x.Next())
          {
            InsertOrUpdateResult result = destination.InsertOrUpdate(x.NouvelleSituation);
            if (logger.IsTraceEnabled)
            {
              string flag = x.NouvelleSituation[Services.Plateau.TailleDescriptionSituationEtMasque - 1].ToString("X").PadLeft(2, '0');
              logger.Trace($"Résultat{nl}{Plateau.Dump(x.NouvelleSituation)}{result} Flag:{flag}");
            }
            switch (result)
            {
              case InsertOrUpdateResult.NoChange:
                nbNoChange++;
                break;
              case InsertOrUpdateResult.Inserted:
                nbInserted++;
                break;
              case InsertOrUpdateResult.Updated:
                nbUpdated++;
                break;
              default:
                break;
            }
          }
          if (LowMemoryState)
          {
            destination.Flush();
            LibereMemoire();
          }
        }
        destination.Flush();
        sw.Stop();
        Parent.Feedback(FeedbackHint.info, $"{Stockages.Count - 1};{nbInserted};{sw.ElapsedMilliseconds}");
        Parent.Feedback(FeedbackHint.info, $"nb situations traitées : {nbSituationsTraitees}, nb nouvelles : {nbInserted}, nb maj : {nbUpdated}, nb sans effet : {nbNoChange} ");
      }
      catch (ApplicationException ex)
      {
        Parent.Feedback(FeedbackHint.error, ex.Message);
      }
      catch (Exception ex)
      {
        Parent.Feedback(FeedbackHint.error, ex.ToString());
      }
      finally
      {
        Parent.Feedback(FeedbackHint.endOfJob, "Arrêt de la tâche de recherche en largeur");
        try
        {
          foreach (BTreePersistant stockage in Stockages)
          {
            stockage.Close();
          }
        }
        catch (Exception ex)
        {
          Parent.Feedback(FeedbackHint.error, "Une erreur est intervenue lors de la fermeture des stocks : " + ex.Message);
        }
      }
    }

    private void LibereMemoire()
    {
      long beforeCollecte = GC.GetTotalMemory(false);
      foreach (var stockage in Stockages)
      {
        stockage.LibereMemoire();
      }
      GC.Collect();
      long afterCollecte = GC.GetTotalMemory(false);
      Parent?.Feedback(FeedbackHint.trace, $"GC before {beforeCollecte}, after {afterCollecte}");
      LowMemoryState = false;
    }

    public void LanceRechercheEnProfondeur()
    {
      Task taskRechercheEnProfondeur = new Task(RechercheEnProfondeur);
      taskRechercheEnProfondeur.Start();

    }

    private void RechercheEnProfondeur()
    {
      Stopwatch sw = Stopwatch.StartNew();
      try
      {
        Parent.Feedback(FeedbackHint.startOfJob, "Recherche en profondeur");
        InitStockage();
        if (Plateau.Reprise != EnumReprise.None)
        {
          Parent.Feedback(FeedbackHint.info, $"Reprise de la recherche");
        }
        InitStockageEnProfondeur();
        long nbScanned = 0, nbDejaResolues = 0;
        byte flagSolutions = Plateau.CalculeSolutionsConnues();
        byte flagSolutionsAll = 0;
        foreach (byte[] situationInitiale in Stockages[0].EnumereElements(null))
        {
          flagSolutionsAll |= situationInitiale[Plateau.TailleDescriptionSituationEtMasque - 1];
        }
        Parent.Feedback(FeedbackHint.info, $"niveau;nb traités;nb déjà résolues;durée(ms)");
        if (Plateau.Reprise == Services.EnumReprise.None) Plateau.SituationReprise = null;
        BTreePersistant origine = Stockages[Stockages.Count - 1];
        if (Etapes.Count > 0)
        {
          foreach (byte[] situation in origine.EnumereElements(Plateau.SituationReprise))
          {
            if (flagSolutions == flagSolutionsAll)
            {
              // TODO : lever dans le fichier pilote le flag Complet
              break;
            }
            nbScanned++;
            if (DemandeSuspension)
            {
              Plateau.EnregistrerSuspension(Repertoire, situation, EnumReprise.EnProfondeur);
              break;
            }
            if (((byte)(situation[Plateau.TailleDescriptionSituationEtMasque - 1] | flagSolutions)) == flagSolutions)
            {
              nbDejaResolues++;
              continue;
            }
            // On initialise le stock volatile des situations qu'on va rencontrer en scrutant cette situation particulière
            // Voir documentation pour les raisons pour lesquelles on ne conserve pas le stock des situations précédemment scrutées.
            foreach (var etape in Etapes)
            {
              etape.Stock = new BTreeVolatile(Plateau.TailleDescriptionSituationEtMasque, Ordre, Plateau);
              etape.Stock.InitBTree();
            }
            Plateau.EnumerationNouvellesSituationsNormalisees x = new Plateau.EnumerationNouvellesSituationsNormalisees(Plateau, situation);
            bool bLoop = true;
            while (bLoop && x.Next())
            {
              int idxProfondeur = 0;
              byte[] situation1 = x.NouvelleSituation;
              Etapes[0].Mvt = x.idxMvtToNouvelleSituation;
              Etapes[0].Enumeration = new Plateau.EnumerationNouvellesSituationsNormalisees(Plateau, situation1);
              for (; idxProfondeur >= 0;)
              {
                if (!Etapes[idxProfondeur].Enumeration.Next())
                {
                  --idxProfondeur;
                  continue;
                }
                byte[] sit = Etapes[idxProfondeur].Enumeration.NouvelleSituation;
                if (idxProfondeur == Etapes.Count - 1)
                {
                  if (Resoluble(sit, origine))
                  {
                    List<int> mvts = Etapes.Select(e => e.Mvt).ToList();
                    mvts.Add(Etapes[idxProfondeur].Enumeration.idxMvtToNouvelleSituation);
                    Plateau.EnregistrerSolutionPartielle(Repertoire, situation, mvts);
                    flagSolutions |= ((byte)sit[Plateau.TailleDescriptionSituationEtMasque - 1]);
                    idxProfondeur = -1;
                    bLoop = false;
                  }
                  continue;
                }
                if (SituationRencontreeRechercheEnProfondeur(sit, idxProfondeur))
                {
                  continue;
                }
                Etapes[idxProfondeur + 1].Mvt = Etapes[idxProfondeur].Enumeration.idxMvtToNouvelleSituation;
                idxProfondeur++;
                Etapes[idxProfondeur].Enumeration = new Plateau.EnumerationNouvellesSituationsNormalisees(Plateau, sit);
              }
            }
            if (LowMemoryState)
            {
              LibereMemoire();
            }
          }
        }
        else
        {
          foreach (byte[] situation in origine.EnumereElements(Plateau.SituationReprise))
          {
            if (flagSolutions == flagSolutionsAll)
            {
              // TODO : lever dans le fichier pilote le flag Complet
              break;
            }
            nbScanned++;
            if (DemandeSuspension)
            {
              Plateau.EnregistrerSuspension(Repertoire, situation, EnumReprise.EnProfondeur);
              break;
            }
            if (((byte)(situation[Plateau.TailleDescriptionSituationEtMasque - 1] | flagSolutions)) == flagSolutions)
            {
              nbDejaResolues++;
              continue;
            }
            Plateau.EnumerationNouvellesSituationsNormalisees x = new Plateau.EnumerationNouvellesSituationsNormalisees(Plateau, situation);
            bool bLoop = true;
            while (bLoop && x.Next())
            {
              byte[] sit = x.NouvelleSituation;
              if (Resoluble(sit, origine))
              {
                List<int> mvts = new List<int>();
                mvts.Add(x.idxMvtToNouvelleSituation);
                Plateau.EnregistrerSolutionPartielle(Repertoire, situation, mvts);
                flagSolutions |= ((byte)sit[Plateau.TailleDescriptionSituationEtMasque - 1]);
                bLoop = false;
              }
            }
          }
        }
        sw.Stop();
        Parent.Feedback(FeedbackHint.info, $"{Stockages.Count - 1};{nbScanned};{nbDejaResolues};{sw.ElapsedMilliseconds}");
      }
      catch (ApplicationException ex)
      {
        Parent.Feedback(FeedbackHint.error, ex.Message);
      }
      catch (Exception ex)
      {
        Parent.Feedback(FeedbackHint.error, ex.ToString());
      }
      finally
      {
        for (int idxStock = 0; idxStock < Stockages.Count; idxStock++)
        {
          Stockages[idxStock].Close();
        }
        Parent.Feedback(FeedbackHint.endOfJob, "Arrêt de la tâche de recherche en profondeur");
      }
    }

    private unsafe bool SituationRencontreeRechercheEnProfondeur(byte[] sit, int idxProfondeur)
    {
      fixed (byte* pSit = sit)
      {
        return Etapes[idxProfondeur + 1].Stock.Existe(pSit);
      }
    }

    // On a obtenu une situation sit dont le nombre de cases libres est égal au nombre de pierres
    // des situations établies dans la dernière couche de la recherche en largeur, obtenues depuis les situations initiales.
    // Si le dual de sit est présent dans cette couche, c'est qu'il existe une liste de mouvements qui mène
    // à au moins une situation duale d'une situation initiale, donc que sit est résoluble.
    private unsafe bool Resoluble(byte[] sit, BTreePersistant origine)
    {
      byte[] dual = Plateau.CalculeDual(sit);
      Plateau.Normalise(dual, out int _);
      fixed (byte* pDual = dual)
      {
        return origine.Existe(pDual);
      }
    }

    private void InitStockageEnProfondeur()
    {
      int nbCases = Plateau.NbCases;
      // Rappel : La liste des stockages est constituée ainsi : 
      // Stockages[0]                 : les situations initiales
      // Stockages[1]                 : les situations possibles après 1 mouvement
      // ...
      // Stockages[Stockages.Count-1] : les situations possibles après Stockages.Count-1 mouvements
      int nbMvtEnLargeur = Stockages.Count - 1;
      // le nombre de pierres d'une situation initiale est nbCases - 1
      // le nombre de pierres après nbMvtEnLargeur est nbCases - 1 - nbMvtEnLargeur
      // il faut effectuer nbMvtEnProfondeur pour atteindre une situation où le nombre de cases libres est nbCases - 1 - nbMvtEnLargeur
      // c'est à dire où le nombre de pierres restantes est nbCases - (nbCases - 1 - nbMvtEnLargeur) = 1 + nbMvtEnLargeur
      // mais ce nombre de pierres restantes est nbCases - 1 - nbMvtEnLargeur - nbMvtEnProfondeur
      // On en déduit la valeur de nbMvtEnProfondeur
      int nbMvtEnProfondeur = nbCases - 2 * (1 + nbMvtEnLargeur);
      // On part d'une situation à nbCases - 1 - nbMvtEnLargeur pierres,
      // pour appliquer nbMvtEnProfondeur mouvements
      // et obtenir une situation à nbCases - 1 - nbMvtEnLargeur cases libres
      // On a besoin de savoir à chaque étape de cette recherche en profondeur si la situation obtenue a déjà été rencontrée
      // sauf pour la dernière étape, où ce qui nous intéresse est de savoir si la situation DUALE a été obtenue
      // (on utilise alors les données du stockage permanent).
      // Donc la taille de la pile Etapes est nbMvtEnProfondeur - 1
      Etapes = new EtapesRechercheEnProfondeur();
      for (int idxEtape = 0; idxEtape < nbMvtEnProfondeur - 1; idxEtape++)
      {
        EtapeRechercheEnProfondeur etape = new EtapeRechercheEnProfondeur();
        Etapes.Add(etape);
      }
    }

    private void InitStockage()
    {
      // On recherche le répertoire portant le n° le + grand
      List<(int prof, DirectoryInfo rep)> Repertoires = new List<(int prof, DirectoryInfo rep)>();
      foreach (var rep in Repertoire.GetDirectories())
      {
        if (int.TryParse(rep.Name, out int prof))
        {
          Repertoires.Add((prof, rep));
        }
        else
        {
          Parent.Feedback(FeedbackHint.warning, $"Un sous-répertoire du répertoire de stockage porte un nom inattendu : {rep.Name}");
        }
      }
      if (Repertoires.Count == 0)
      {
        throw new ApplicationException($"Le contenu du répertoire de stockage est incorrect");
      }
      Repertoires.Sort(((int prof, DirectoryInfo rep) a, (int prof, DirectoryInfo rep) b) => a.prof.CompareTo(b.prof));
      if (Repertoires[0].prof != 0)
      {
        throw new ApplicationException($"Le contenu du répertoire de stockage est incorrect");
      }
      for (int idxRep = 1; idxRep < Repertoires.Count; idxRep++)
      {
        if (Repertoires[idxRep].prof != Repertoires[idxRep - 1].prof + 1)
        {
          throw new ApplicationException($"Le contenu du répertoire de stockage est incorrect");
        }
      }
      Stockages = new List<BTreePersistant>();
      for (int idxRep = 0; idxRep < Repertoires.Count; idxRep++)
      {
        Stockages.Add(new BTreePersistant(Repertoires[idxRep].rep, Services.Plateau.TailleDescriptionSituationEtMasque, Ordre, Plateau));
        Stockages[Stockages.Count - 1].InitBTree();
      }
    }

    private void IncrementeStockage()
    {
      DirectoryInfo rep = new DirectoryInfo(Path.Combine(Repertoire.FullName, $"{Stockages.Count}"));
      Stockages.Add(new BTreePersistant(rep, Services.Plateau.TailleDescriptionSituationEtMasque, Ordre, Plateau));
      Stockages[Stockages.Count - 1].InitBTree();
    }

    public void Charger()
    {
      Plateau = Services.Plateau.ChargerFichierPilote(Repertoire);
      Parent.Feedback(FeedbackHint.trace, $"Plateau chargé :{Environment.NewLine}{Plateau.DescriptionPlateau()}");
    }

    private void BuildStats()
    {
      try
      {
        InitStockage();
        Parent.Feedback(FeedbackHint.startOfJob, $"Collecte de statistiques du niveau {Stockages.Count - 1}");
        long[] tblParSituationInitiale = new long[8]; // cases initialisées à 0 par l'opérateur new
        long[] tblParFlag = new long[256]; // cases initialisées à 0 par l'opérateur new
        int niveau = Stockages.Count - 1;
        Stopwatch sw = Stopwatch.StartNew();
        foreach (var situation in Stockages[niveau].EnumereElements(null))
        {
          // Décompte par situation initiale
          byte f = situation[Plateau.TailleDescriptionSituationEtMasque - 1];
          for (int i = 0, j = 1; i < 8; i++, j *= 2)
          {
            if ((f & j) != 0)
            {
              tblParSituationInitiale[i]++;
            }
          }
          // Décompte par groupe différent de situations initiales
          tblParFlag[f]++;
        }
        sw.Stop();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Statistiques du niveau {niveau} ");
        sb.AppendLine("Statistiques par situation initiale");
        sb.AppendLine("flag;nombre");
        for (int i = 0, j = 1; i < 8; i++, j *= 2)
        {
          string fmtBinaire = System.Convert.ToString(j, 2).PadLeft(8, '0');
          sb.AppendLine($"{fmtBinaire};{tblParSituationInitiale[i]}");
        }
        sb.AppendLine();
        sb.AppendLine("Statistiques par groupement de situations initiales");
        sb.AppendLine("flag;nombre");
        for (int i = 0; i < 256; i++)
        {
          if (tblParFlag[i] != 0)
          {
            string fmtBinaire = System.Convert.ToString(i, 2).PadLeft(8, '0');
            sb.AppendLine($"{fmtBinaire};{tblParFlag[i]}");
          }
        }
        sb.AppendLine();
        sb.AppendLine($"scrutation en {sw.ElapsedMilliseconds / 1000} secondes");
        Parent?.Feedback(FeedbackHint.info, sb.ToString());
      }
      catch (ApplicationException ex)
      {
        Parent.Feedback(FeedbackHint.error, ex.Message);
      }
      catch (Exception ex)
      {
        Parent.Feedback(FeedbackHint.error, ex.ToString());
      }
      finally
      {
        try
        {
          foreach (var stock in Stockages)
          {
            stock.Close();
          }
        }
        catch (Exception ex)
        {
          Parent.Feedback(FeedbackHint.error, $"Erreur lors de la libération du stock : {ex.Message}");
        }
        Parent.Feedback(FeedbackHint.endOfJob, "Arrêt de la tâche de collecte de statistiques");
      }
    }

    private void Verifier()
    {
      try
      {
        InitStockage();
        Parent.Feedback(FeedbackHint.startOfJob, $"Vérification du stock");
        for (int idxCouche = 15; idxCouche < Stockages.Count; idxCouche++)
        {
          BTreePersistant A = Stockages[idxCouche];
          BTreePersistant B = null;
          if (idxCouche < Stockages.Count - 1)
          {
            B = Stockages[idxCouche + 1];
          }
          long nbSituations = 0;
          byte[] situationPrecedente = new byte[Plateau.TailleDescriptionSituationEtMasque];
          byte[] situationNormalisee = new byte[Plateau.TailleDescriptionSituationEtMasque];
          foreach (byte[] situation in A.EnumereElements(null))
          {
            Array.Copy(situation, situationNormalisee, Plateau.TailleDescriptionSituationEtMasque);
            Plateau.Normalise(situationNormalisee, out int idxSymetrieAppliquee);
            if (idxSymetrieAppliquee != -1)
            {
              throw new ApplicationException("Découverte d'une situation non normalisée dans le stock");
            }
            if (nbSituations > 0)
            {
              int nCmp = Plateau.CompareSituations(situationPrecedente, situation);
              if (nCmp >= 0)
              {
                throw new ApplicationException("Découverte de deux situations non classées dans le stock");
              }
            }
            if (idxCouche < Stockages.Count - 1)
            {
              var x = new Plateau.EnumerationNouvellesSituationsNormalisees(Plateau, situation);
              while (x.Next())
              {
                byte[] situationDerivee = x.NouvelleSituation;
                Array.Copy(situationDerivee, situationNormalisee, Plateau.TailleDescriptionSituationEtMasque);
                Plateau.Normalise(situationNormalisee, out idxSymetrieAppliquee);
                if (idxSymetrieAppliquee != -1)
                {
                  throw new ApplicationException("L'itérateur de nouvelles situations a rendu une situation non normalisée");
                }
                bool bExiste = B.Existe(situationDerivee);
                if (!bExiste)
                {
                  throw new ApplicationException("Une situation dérivée n'a pas été trouvée dans le stock suivant");
                }
              }
            }
            nbSituations++;
            Array.Copy(situation, situationPrecedente, Plateau.TailleDescriptionSituationEtMasque);
            if (LowMemoryState)
            {
              LibereMemoire();
            }
            if (DemandeSuspension)
            {
              break;
            }
          }
          Parent.Feedback(FeedbackHint.info, $"Niveau {idxCouche} vérifié, {nbSituations} scannées");
          if (DemandeSuspension)
          {
            Parent.Feedback(FeedbackHint.info, "Arrêt de la vérification");
            break;
          }
        }
      }
      catch (ApplicationException ex)
      {
        Parent.Feedback(FeedbackHint.error, ex.Message);
      }
      catch (Exception ex)
      {
        Parent.Feedback(FeedbackHint.error, ex.ToString());
      }
      finally
      {
        try
        {
          foreach (var stock in Stockages)
          {
            stock.Close();
          }
        }
        catch (Exception ex)
        {
          Parent.Feedback(FeedbackHint.error, $"Erreur lors de la libération du stock : {ex.Message}");
        }
        Parent.Feedback(FeedbackHint.endOfJob, "Arrêt de la tâche de vérification du stock");
      }
    }


    public List<SolutionDetaillee> GetSolutionsDetaillees()
    {
      return Plateau.GetSolutionsDetaillees();
    }
    public void TestMouvementsOriginels()
    {
      Plateau.TestMouvementsOriginaux();
    }
  }
}
