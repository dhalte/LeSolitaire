using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace LeSolitaireLogiqueV0
{
  public class Logique
  {
    // Permet l'envoie de signaux affichables à la fenêtre parent
    // Et en particulier les exceptions et un EndOfJob
    private IFeedback Parent;
    // Gestion de la demande d'arrêt prématurée de la tâche de fond
    public enumState State { get; private set; } = enumState.stopped;
    // Les opérations possibles affichables dans le menu des actions (bitfield)
    public enumOp EnumOp { get; private set; }
    // La tâche de fond (il n'est pas strictement utile de la conserver dans un champ)
    private Task BgTask;
    // Regroupement de paramètres de configuration
    public LogiqueConfiguration Config;
    // Le plateau de jeu
    private Plateau Plateau;
    // Tableau de buffers permettant la manipulation d'une situation
    // On gère un tel tableau car on fait une recherche en profondeur récursive de taille connue
    // et on ne veut pas être continuellement en train d'allouer des buffers sur la stack.
    private SituationsEtude SituationsEtude;
    // Suivi des situations déjà rencontrées
    public SituationStock SituationStock;
    // Liste des situations gagnantes (utile uniquement pour construire EF.dat)
    private List<SituationInitiale> EG;
    // Petit utilitaire permettant d'émettre des infos régulières, de + en + espacées
    private ScheduleInfo ScheduleInfo = new ScheduleInfo();
    // Pour la statistique délivrée par ScheduleInfo
    private long NbSD;
    // tableau des mouvements effectués
    private (int idxOrigine, int idxSaut, int idxDestination)[] Mouvements;
    public bool LowMemory;

    public Logique(IFeedback parent)
    {
      Parent = parent;
      Config = new LogiqueConfiguration();
    }

    // Calcul de la liste des actions possibles et utiles
    // Calcul partiel car il doit rester synchrone et rapide, 
    // pour afficher le plus rapidement possible la liste des actions.
    public enumOp Verifie(string filenameFicheDeJeu)
    {
      EnumOp = Config.Verifie(Parent, filenameFicheDeJeu);
      if (EnumOp != enumOp.Initialiser)
      {
        Plateau = new Plateau(Config.Pilote.PlateauRaw);
      }
      if (EnumOp != enumOp.Initialiser)
      {
        CalculeSituationsGagnantes();
        if (Config.Pilote.PreSolutions.Count + Config.Pilote.Solutions.Count < EG.Count)
        {
          EnumOp |= enumOp.Rechercher;
          if (Config.Pilote.PreSolutions.Count == 0)
          {
            EnumOp |= enumOp.ReglerNF;
          }
          if (Config.Pilote.PreSolutions.Count > 0 || Config.Pilote.Solutions.Count(s => !s.Complete) > 0)
          {
            EnumOp |= enumOp.ConsoliderSolutions;
          }
        }
        else
        {
          EnumOp &= ~enumOp.Rechercher;
        }
      }
      return EnumOp;
    }

    // Permet de lever le flag que la tâche de fond surveille
    // afin de s'arrêter prématurément si la demande lui en est faite.
    public void StoppeBgTask()
    {
      if (State != enumState.running)
      {
        Parent?.Feedback(enumFeedbackHint.error, "processus a l'arrêt ou en cours d'arrêt");
        return;
      }
      if (BgTask != null && BgTask.IsCompleted)
      {
        Parent?.Feedback(enumFeedbackHint.info, "La tâche de fond est terminée");
        State = enumState.stopped;
      }
      else
      {
        State = enumState.stopping;
      }
    }

    // Lancement de la tâche de fond d'initialisation.
    // Celle-ci est rapide et pourrait être synchrone.
    public void LanceInitialiser(string filenameFicheDeJeu)
    {
      if (State != enumState.stopped)
      {
        Parent?.Feedback(enumFeedbackHint.error, "processus en cours");
        return;
      }
      Parent?.Feedback(enumFeedbackHint.info, "Début initialisation");
      State = enumState.running;
      BgTask = Task.Run(() => Initialiser(filenameFicheDeJeu));
    }

    // Crée au besoin répertoire, fichier pilote, fichiers initiaux ED.dat / EF.dat
    private void Initialiser(string filenameFicheDeJeu)
    {
      try
      {
        if (Config.Initialiser(Parent, filenameFicheDeJeu))
        {
          Plateau = new Plateau(Config.Pilote.PlateauRaw);
          SituationStock = new SituationStock(Plateau.NbCasesPlateau);
          SituationsEtude = new SituationsEtude(Plateau);
          CalculeSituationsGagnantes();
          InitialiserEF();
          Config.SauvePilote();
        }
      }
      catch (Exception ex)
      {
        Parent?.Feedback(enumFeedbackHint.error, $"{ex}");
      }
      State = enumState.stopped;
      Parent?.Feedback(enumFeedbackHint.endOfJob, "Fin du job");
    }

    private void InitialiserEF()
    {
      using (MyContext myContext = new MyContext())
      {
        int nfInDB = myContext.VerifieEF();
        if (nfInDB > 0 && nfInDB != Config.Pilote.Nf)
        {
          Parent?.Feedback(enumFeedbackHint.error, $"Incohérence entre NF du pilote et NF dans la DB");
          nfInDB = -1;

        }
        if (nfInDB < 0)
        {
          myContext.CreateEFtmp(1);
          byte idxSG = 1;
          foreach (Situation situationGagnante in EG)
          {
            myContext.InsertEFtmp(situationGagnante.Pierres, idxSG);
            string trace = $"idxSG={Common.ConvertFlags(idxSG)}\r\n{situationGagnante.Dump(Plateau)}";
            Parent?.Feedback(enumFeedbackHint.trace, trace);
            idxSG <<= 1;
          }
          myContext.renameEFtmp();
        }
        Config.Pilote.ChangeNF(0);
        Config.SauvePilote();
      }
    }

    // Lance la constrution du fichier EF.dat pour une valeur donnée du paramètre NF
    public void LanceReglerNF(int nf)
    {
      if (State != enumState.stopped)
      {
        Parent?.Feedback(enumFeedbackHint.error, "processus en cours");
        return;
      }
      Parent?.Feedback(enumFeedbackHint.info, $"Début réglage de ND à la valeur {nf}");
      State = enumState.running;
      BgTask = Task.Run(() => ReglerNF(nf));
    }

    private void ReglerNF(int nf)
    {
      try
      {
        if (nf < Config.Pilote.Nf)
        {
          // Si c'est nécessaire pour ED (à cause de son tableau de liens avec les SI),
          // ça ne l'est pas vraiment pour EF, où on pourrait recalculer les EF.dat précédents à partir du courant.
          Parent?.Feedback(enumFeedbackHint.info, $"NF décroit, on doit réinitialiser NF à 1");
          InitialiserEF();
        }
        ScheduleInfo.Start();
        while (nf > Config.Pilote.Nf && State == enumState.running)
        {
          Parent?.Feedback(enumFeedbackHint.info, $"incrémentation de NF de {Config.Pilote.Nf} à {Config.Pilote.Nf + 1}");
          if (!IncrementerNF())
          {
            break;
          }
        }
      }
      catch (Exception ex)
      {
        Parent?.Feedback(enumFeedbackHint.error, $"{ex}");
      }
      State = enumState.stopped;
      Parent?.Feedback(enumFeedbackHint.endOfJob, "Fin du job");
    }
    private bool IncrementerNF()
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      SituationEtude situationEtude = new SituationEtude(Plateau);
      // NF est le nombre de mouvements qu'il reste à faire pour ne laisser qu'une pierre
      // Il y a NF+1 pierres dans les situations inscrites dans la table EF
      // Et on veut la remplacer par la table contenant les situations finales à NF+2 pierres
      int nbPierres = Config.Pilote.Nf + 1;
      int nbPierresFutures = nbPierres + 1;
      byte[] idxPierresFutures = new byte[nbPierresFutures];
      using (MyContext myContextReadAll = new MyContext(Config.Pilote.DBname))
      {
        myContextReadAll.CreateEFtmp(nbPierresFutures);
        using (MyContext myContextTraiteOne = new MyContext(Config.Pilote.DBname))
        {
          foreach ((byte[] situation, byte sg) situationEF in myContextReadAll.ReadAllEF(nbPierres))
          {
            situationEtude.ChargeSituation(situationEF.situation);
            foreach (var mvt in Plateau.MouvementsPossibles)
            {
              if (situationEtude.MouvementInversePossible(mvt))
              {
                situationEtude.EffectueMouvementInverse(mvt);
                situationEtude.CalculeSituationMinimale(Plateau);
                Common.Convert(situationEtude.ImagePierres, idxPierresFutures);
                //string trace = Common.Dump(idxPierresFutures, Plateau);
                //Parent?.Feedback(enumFeedbackHint.trace, trace);
                myContextTraiteOne.UpdateOrInsert(idxPierresFutures, situationEF.sg);
                situationEtude.EffectueMouvement(mvt);
              }
            }
          }
        }
        myContextReadAll.renameEFtmp();
      }
      Config.Pilote.IncrementerNF();
      Config.SauvePilote();
      stopwatch.Stop();
      Parent?.Feedback(enumFeedbackHint.trace, $"Incrémentation de NF -> {Config.Pilote.Nf} en {stopwatch.Elapsed}");
      // On dump le nombre de SF regroupées par mêmes SG
      Parent?.Feedback(enumFeedbackHint.trace, $"sg\tnb");
      stopwatch.Restart();
      using (MyContext myContextDump = new MyContext(Config.Pilote.DBname))
      {
        foreach ((byte sg, long nbSF) item in myContextDump.DumpSFgroupbySG())
        {
          Parent?.Feedback(enumFeedbackHint.trace, $"'{Common.ConvertFlags(item.sg)}\t{item.nbSF}");
        }
      }
      stopwatch.Stop();
      Parent?.Feedback(enumFeedbackHint.trace, $"Enumération des combinaisons de SG et du nombre de SF associées -> {stopwatch.Elapsed}");
      return true;
    }
    private bool IncrementerNFOld()
    {
      HashSet<SituationBase> SFnew = new HashSet<SituationBase>();
      SituationEtude situationEtude = new SituationEtude(Plateau);
      // En lecture
      using (FileStream EFold = Config.OpenData(enumAccesData.EFdat))
      {
        int tailleSituationFinaleRaw = Config.TailleSituationsNF;
        byte[] situationFinaleRaw = new byte[tailleSituationFinaleRaw];
        long nbSFold = EFold.Length / (tailleSituationFinaleRaw);
        for (int idxOld = 0; idxOld < nbSFold; idxOld++)
        {
          int n = EFold.Read(situationFinaleRaw, 0, tailleSituationFinaleRaw);
          if (n != tailleSituationFinaleRaw)
          {
            Parent?.Feedback(enumFeedbackHint.error, "Erreur de lecture de EF.dat");
            return false;
          }
          situationEtude.ChargeSituation(situationFinaleRaw);
          foreach (var mvt in Plateau.MouvementsPossibles)
          {
            if (situationEtude.MouvementInversePossible(mvt))
            {
              situationEtude.EffectueMouvementInverse(mvt);
              Situation situationFinale = TryGetSituation(situationEtude, SFnew) as Situation;
              if (situationFinale == null)
              {
                Situation situationNew = situationEtude.NewSituation();
                SFnew.Add(situationNew);
              }
              situationEtude.EffectueMouvement(mvt);
            }
          }
          if (ScheduleInfo.DelivreInfo())
          {
            Parent?.Feedback(enumFeedbackHint.info, $"Calcul de {SFnew.Count} situations");
          }
          if (State != enumState.running)
          {
            Parent.Feedback(enumFeedbackHint.info, "Demande d'arrêt prise en compte");
            return false;
          }
        }
      }

      // En écriture
      using (FileStream EFnew = Config.OpenData(enumAccesData.EFtmp))
      {
        foreach (Situation situationFinale in SFnew)
        {
          EFnew.Write(situationFinale.Pierres, 0, situationFinale.Pierres.Length);
        }
        Parent?.Feedback(enumFeedbackHint.info, $"Enregistrement de {SFnew.Count} situations");
      }
      Config.ReplaceData(enumAccesData.EFdat);
      Config.Pilote.IncrementerNF();
      Config.SauvePilote();
      Config.TailleSituationsNF++;
      return true;
    }

    // Lance la recherche à partir des SD de ED.dat
    public void LanceRecherche()
    {
      if (State != enumState.stopped)
      {
        Parent?.Feedback(enumFeedbackHint.error, "processus en cours");
        return;
      }
      State = enumState.running;
      BgTask = new Task(Recherche);
      BgTask.Start();
    }

    void Recherche()
    {
      try
      {
        if (!RechercheInitialisation())
        {
          State = enumState.stopped;
          Parent?.Feedback(enumFeedbackHint.endOfJob, "Fin du job");
          return;
        }
        using (FileStream EDdat = Config.OpenData(enumAccesData.EDdat))
        {
          NbSD = EDdat.Length / (Plateau.NbCasesPlateau - Config.TailleSituationsND + EG.Count);
        }

        Parent?.Feedback(enumFeedbackHint.info, $"Début recherche nd={Config.Pilote.Nd}, nf={Config.Pilote.Nf}, nb SD={NbSD} idxReprise={Config.Pilote.IdxReprise} ({100f * Config.Pilote.IdxReprise / NbSD} %)");
        int tailleDualSituationDepartRaw = Plateau.NbCasesPlateau - Config.TailleSituationsND;
        byte[] dualSituationDepartRaw = new byte[tailleDualSituationDepartRaw];
        int nbSituationsInitiales = EG.Count;
        byte situationsInitialesAssociees;
        ScheduleInfo.Start();
        // En lecture
        bool bToutesSIresolues = RechercheSiToutesSituationsInitialesResolues();
        using (FileStream EDdat = Config.OpenData(enumAccesData.EDdat))
        {
          long offsetFichierED = Config.Pilote.IdxReprise * (tailleDualSituationDepartRaw + nbSituationsInitiales);
          if (offsetFichierED >= EDdat.Length)
          {
            Parent?.Feedback(enumFeedbackHint.info, "Fin de lecture de ED.dat");
          }
          else
          {
            Stopwatch swStats = Stopwatch.StartNew();
            long idxOld = Config.Pilote.IdxReprise;
            EDdat.Seek(offsetFichierED, SeekOrigin.Begin);
            Parent?.Feedback(enumFeedbackHint.trace, "tps % idx");
            while (State == enumState.running && !bToutesSIresolues)
            {
              if (!LitEntreeED(EDdat, dualSituationDepartRaw, out situationsInitialesAssociees, Plateau))
              {
                break;
              }
              if (!RechercheNouvelleSIpossible(situationsInitialesAssociees))
              {
                // Cette SD adresse des SI déjà résolues, inutile de la tester.
                Config.Pilote.IdxReprise++;
                continue;
              }
              byte[] situationDepartRaw = Plateau.SituationDualeRaw(dualSituationDepartRaw);
              Situation situationDepart = new Situation(situationDepartRaw);
              if (Recherche(situationDepart))
              {
                // On a relié SD à une SF. On enregistre la pré-solution.
                // RQ : les tests précédents assurent qu'au moins une SI non encore résolue est concernée.
                PreSolution preSolution = new PreSolution();
                preSolution.IdxSD = Config.Pilote.IdxReprise;
                int nbSIresolues = 0;
                for (int idxSituationInitiale = 0; idxSituationInitiale < nbSituationsInitiales; idxSituationInitiale++)
                {
                  byte flag = (byte)(1 << idxSituationInitiale);
                  if ((situationsInitialesAssociees & flag) == flag)
                  {
                    if (!EG[idxSituationInitiale].Resolue)
                    {
                      preSolution.IdxSIlist.Add(idxSituationInitiale);
                      EG[idxSituationInitiale].Resolue = true;
                      nbSIresolues++;
                    }
                  }
                }
                for (int idxMvt = Config.TailleSituationsND; idxMvt > Config.TailleSituationsNF; idxMvt--)
                {
                  (int idxDepart, int idxSaut, int idxArrivee) mvt = Mouvements[idxMvt];
                  SolutionMouvement solutionMouvement = new SolutionMouvement((byte)mvt.idxDepart, (byte)mvt.idxSaut);
                  preSolution.Mouvements.Add(solutionMouvement);
                }
                Config.Pilote.PreSolutions.Add(preSolution);
                Config.SauvePilote();
                Parent?.Feedback(enumFeedbackHint.info, $"Pré-solution trouvée pour {nbSIresolues} nouvelle(s) SI");
                bToutesSIresolues = RechercheSiToutesSituationsInitialesResolues();
              }

              if (State == enumState.running)
              {
                Config.Pilote.IdxReprise++;
              }
              if (LowMemory)
              {
                LibereMemoire();
                LowMemory = false;
              }
              if (ScheduleInfo.DelivreInfo())
              {
                // Durée en fraction de journée, conformément à Excel
                double duree = swStats.Elapsed.TotalSeconds / 3600.0 / 24.0;
                long idxNew = Config.Pilote.IdxReprise, idxDelta = idxNew - idxOld;
                Parent?.Feedback(enumFeedbackHint.trace, $"{duree} {100f * idxDelta / NbSD} {idxDelta}");
                swStats.Restart();
                idxOld = idxNew;
              }
            }

          }
        }
        if (bToutesSIresolues)
        {
          Parent?.Feedback(enumFeedbackHint.info, $"Toutes les situations initiales ont été résolues !");
        }
      }
      catch (Exception ex)
      {
        Parent?.Feedback(enumFeedbackHint.error, $"{ex}");
      }
      finally
      {
        Config?.SauvePilote();
      }

      State = enumState.stopped;
      Parent?.Feedback(enumFeedbackHint.endOfJob, "Fin du job");
    }

    private void LibereMemoire()
    {
      // Parent.Feedback(enumFeedbackHint.info, $"{DateTime.Now:HH:mm:ss.fff} >LibereMemoire Memory used before collection:{GC.GetTotalMemory(false)}");
      SituationStock.LibereMemoire(Config.TailleSituationsNF + 1, Config.TailleSituationsND);
      GC.Collect();
      // Parent.Feedback(enumFeedbackHint.info, $"{DateTime.Now:HH:mm:ss.fff} >LibereMemoire Memory used after full collection:{GC.GetTotalMemory(true)}");
    }

    // Teste si toutes les SI de EI sont marquées comme résolues
    private bool RechercheSiToutesSituationsInitialesResolues()
    {
      return !EG.Any(si => !si.Resolue);
    }

    // Lorsqu'on teste une SD, on veut s'assurer qu'elle peut être utile pour au moins une SI.
    // Or si toutes les SI qui mènent à cette SD sont déjà résolues, il est inutile d'étudier cette SD.
    // on renvoie un compte-rendu, une chaine de EI.Count digits :
    //   0 : la SI ne concerne pas cette SD
    //   1 : la SI concerne cette SD et n'est pas encore résolue
    //   2 : la SI concerne cette SD mais est déjà résolue
    private bool RechercheNouvelleSIpossible(byte situationsInitialesAssociees)
    {
      for (int idxSI = 0; idxSI < EG.Count; idxSI++)
      {
        byte flagEI = (byte)(1 << idxSI);
        if ((situationsInitialesAssociees & flagEI) == flagEI)
        {
          if (!EG[idxSI].Resolue)
          {
            return true;
          }
        }
      }
      return false;
    }

    private bool RechercheInitialisation()
    {
      SituationsEtude = new SituationsEtude(Plateau);
      SituationStock = new SituationStock(Plateau.NbCasesPlateau);
      Mouvements = new (int idxOrigine, int idxSaut, int idxDestination)[Plateau.NbCasesPlateau];
      MarqueSituationsInitialesResolues(true);
      RechercheChargeEF();
      return true;
    }

    private void CalculeSituationsGagnantes()
    {
      EG = new List<SituationInitiale>();
      int nbCases = Config.Pilote.PlateauRaw.Count;
      SituationEtude situationEtude = new SituationEtude(Plateau);
      HashSet<SituationBase> hEG = new HashSet<SituationBase>();
      foreach (byte idxCase in Plateau.Cases)
      {
        // Pour chaque case du plateau vide, on place la pierre afin de constituer une situation gagnante possible
        SituationRaw situationInitialeRaw = new SituationRaw();
        (int x, int y) coordonnee = Plateau.Etendue.FromByte(idxCase);
        // En fait, pour ce que l'on veut en faire, 
        // il n'est pas nécessaire de conserver dans la description une case si elle est vide.
        situationInitialeRaw.Add((coordonnee.x, coordonnee.y, true));
        situationEtude.ChargeSituationRaw(situationInitialeRaw);
        situationEtude.CalculeSituationMinimale(Plateau);

        SituationInitiale situationGagnante = situationEtude.NewSituationImage();
        // Si aucune symétrie de situationInitialeRaw n'est encore dans le stock, y ajouter situationInitialeRaw
        if (!hEG.Contains(situationGagnante))
        {
          hEG.Add(situationGagnante);
          EG.Add(situationGagnante);
        }
      }
    }

    private bool SituationEtudieeExisteDeja(SituationEtude situationEtude)
    {
      bool bExiste = false;
      for (int idxSymetrie = 0; idxSymetrie < Plateau.NbSymetries; idxSymetrie++)
      {
        Plateau.GenereSymetrie(situationEtude, idxSymetrie);
        if (SituationStock.Contains(situationEtude))
        {
          bExiste = true;
          break;
        }
      }
      return bExiste;
    }

    private SituationBase TryGetSituation(SituationEtude situationEtude, HashSet<SituationBase> stock)
    {
      bool bExiste = false;
      SituationBase situation = null;
      for (int idxSymetrie = 0; idxSymetrie < Plateau.NbSymetries; idxSymetrie++)
      {
        Plateau.GenereSymetrie(situationEtude, idxSymetrie);
        if (stock.TryGetValue(situationEtude, out situation))
        {
          bExiste = true;
          break;
        }
      }

      return bExiste ? situation : null;
    }

    // Au début de la recherche, on charge dans le stock des situations déjà rencontrées les SF de EF
    private bool RechercheChargeEF()
    {
      Parent?.Feedback(enumFeedbackHint.info, "Début du chargement des situations finales");
      ScheduleInfo.Start();
      int cptSituationsLues = 0;
      using (FileStream EFdat = Config.OpenData(enumAccesData.EFdat))
      {
        long sizeEF = EFdat.Length;
        if ((sizeEF % Config.TailleSituationsNF) != 0 || sizeEF == 0)
        {
          Parent?.Feedback(enumFeedbackHint.error, "La taille de EF.dat est incorrecte");
          return false;
        }
        byte[] situationRaw = new byte[Config.TailleSituationsNF];
        for (; ; )
        {
          int octetsLus = EFdat.Read(situationRaw, 0, Config.TailleSituationsNF);
          if (octetsLus == 0)
          {
            break;
          }
          if (octetsLus != Config.TailleSituationsNF)
          {
            Parent?.Feedback(enumFeedbackHint.error, $"La taille de EF.dat n'est pas un multiple de {Config.TailleSituationsNF}");
            return false;
          }
          foreach (byte idxCase in situationRaw)
          {
            if (!Plateau.Contains(idxCase))
            {
              Parent?.Feedback(enumFeedbackHint.error, $"Les données de EF.dat sont incohérentes.");
              return false;
            }
          }
          Situation situationFinale = new Situation(situationRaw);
          SituationStock.Add(situationFinale);
          cptSituationsLues++;
          if (State != enumState.running)
          {
            return false;
          }
          if (ScheduleInfo.DelivreInfo())
          {
            Parent?.Feedback(enumFeedbackHint.info, $"{cptSituationsLues} situations chargées");
          }
        }
      }

      Parent?.Feedback(enumFeedbackHint.info, $"{cptSituationsLues} situations chargées");
      Parent?.Feedback(enumFeedbackHint.info, "Fin du chargement des situations finales");
      return true;
    }

    // Comparer les situations de EI avec celles de la liste des solutions et celle des pré-solutions déjà découvertes
    // Processus : les EI sont générées à l'initialisation
    // Puis lors de la recherche, une situation de ED matche une situation de EF (à une symétrie près)
    // On cherche alors les situations de EI (non encore résolues) qui matchent
    private void MarqueSituationsInitialesResolues(bool bInclutPreSolutions)
    {
      if (bInclutPreSolutions)
      {
        // Les SI résolues, mais pas encore consolidées
        foreach (PreSolution preSolution in Config.Pilote.PreSolutions)
        {
          foreach (int idxPreSolution in preSolution.IdxSIlist)
          {
            EG[idxPreSolution].Resolue = true;
          }
        }
      }
      // Les solutions obtenues ne partent pas nécessairement d'un SI de EI, 
      // mais éventuellement d'une situation égale à une symétrie près d'un SI de EI
      // Contruction d'un Hashset pour utiliser TryGetSituation()
      HashSet<SituationBase> hSolutions = new HashSet<SituationBase>();
      // Le chargement du pilote se contente de calculer les tableaux d'indices de la SI de la solution (SituationRaw),
      // elle ne construit pas un objet Situation
      SituationEtude situationEtude = new SituationEtude(Plateau);
      foreach (Solution solution in Config.Pilote.Solutions)
      {
        if (solution.Complete || bInclutPreSolutions)
        {
          Situation situationInitialeSolution = new Situation(Plateau.Etendue, solution.SituationInitialeRaw);
          solution.SituationInitiale = situationInitialeSolution;
          hSolutions.Add(solution.SituationInitiale);
        }
      }
      foreach (SituationInitiale situationInitiale in EG.FindAll(si => !si.Resolue))
      {
        situationEtude.ChargeSituation(situationInitiale);
        if (TryGetSituation(situationEtude, hSolutions) != null)
        {
          situationInitiale.Resolue = true;
        }
      }
    }

    private bool Recherche(Situation situation)
    {
      SituationEtude situationEtude = SituationsEtude[situation.NbPierres];
      situationEtude.ChargeSituation(situation);
      foreach (var mvt in Plateau.MouvementsPossibles)
      {
        if (situationEtude.MouvementPossible(mvt))
        {
          situationEtude.EffectueMouvement(mvt);
          // Le stock concernant les SF est initialisé avec les SF "gagnantes" de EF.dat
          // il ne doit pas être modifié ici.
          if (!SituationEtudieeExisteDeja(situationEtude))
          {
            if (situationEtude.NbPierres > Config.TailleSituationsNF)
            {
              // Cas général, la situation est nouvelle, on n'est pas arrivé à NF
              Mouvements[situation.NbPierres] = mvt;
              Situation situationNew = situationEtude.NewSituation();
              SituationStock.Add(situationNew);
              if (Recherche(situationNew))
              {
                return true;
              }
            }
          }
          else
          {
            if (situationEtude.NbPierres == Config.TailleSituationsNF)
            {
              // On vient de trouver une situation NF, on a trouvé une solution
              Parent?.Feedback(enumFeedbackHint.info, $"Situation EF trouvée");
              Mouvements[situation.NbPierres] = mvt;
              Situation situationNew = situationEtude.NewSituation();
              return true;
            }
          }
          situationEtude.EffectueMouvementInverse(mvt);
          if (State != enumState.running)
          {
            return false;
          }
        }
      }
      return false;
    }

    // Lance la consolidation des présolutions
    public void LanceConsolider()
    {
      if (State != enumState.stopped)
      {
        Parent?.Feedback(enumFeedbackHint.error, "processus en cours");
        return;
      }
      Parent?.Feedback(enumFeedbackHint.info, "Début consolidation");
      State = enumState.running;
      BgTask = new Task(Consolider);
      BgTask.Start();
    }

    // Traite les pré-solutions et les solutions incomplètes
    //   Pré-solution : recherche les SI pouvant être à l'origine de la SD (à une symétrie près)
    //     Pour chaque SI trouvée qui n'a pas encore de solution (complète ou non),
    //     inscrit une solution incomplète qui part de cette SI et les mouvements qui mènent de cette SI à la SF associée à la SD
    //   Solution incomplète : recherche les mouvements qui mènent de la SF à une SG 
    //     et les inscrit à la suite des mouvements de la solution incomplète.
    private void Consolider()
    {
      try
      {
        MarqueSituationsInitialesResolues(false);
        SituationStock = new SituationStock(Plateau.NbCasesPlateau);
        SituationsEtude = new SituationsEtude(Plateau);
        Mouvements = new (int idxOrigine, int idxSaut, int idxDestination)[Plateau.NbCasesPlateau];
        // On va modifier la collection. Pas de foreach
        while (Config.Pilote.PreSolutions.Count > 0)
        {
          PreSolution preSolution = Config.Pilote.PreSolutions[0];
          List<Solution> solutions = Consolider(preSolution);
          if (State != enumState.running)
          {
            Parent?.Feedback(enumFeedbackHint.info, "Prise en compte de la demande d'arrêt");
            break;
          }
          foreach (Solution solution in solutions)
          {
            // Il faudra les reprendre pour trouver les mouvements finals
            solution.Complete = false;
            solution.Mouvements.AddRange(preSolution.Mouvements);
          }
          Config.Pilote.Solutions.AddRange(solutions);
          Config.Pilote.PreSolutions.RemoveAt(0);
          Config.SauvePilote();
        }
        if (State == enumState.running)
        {
          foreach (Solution solution in Config.Pilote.Solutions)
          {
            if (!solution.Complete)
            {
              // On reconstruit la situation SF à partir de la SI et des mouvements
              SituationRaw situationRaw = solution.SituationInitialeRaw;
              Situation situation = new Situation(Plateau.Etendue, situationRaw);
              foreach (SolutionMouvement solutionMouvement in solution.Mouvements)
              {
                SituationEtude situationEtude = SituationsEtude[situation.NbPierres];
                situationEtude.ChargeSituation(situation);
                situationEtude.EffectueMouvement((solutionMouvement.IdxDepart, solutionMouvement.IdxSaut, solutionMouvement.IdxArrivee(Plateau.Etendue)));
                situation = situationEtude.NewSituation();
              }
              // Ne pas se laisser influencer par les situations déjà rencontrées.
              SituationStock = new SituationStock(Plateau.NbCasesPlateau);
              // Remplir le tableau Mouvements avec des mouvements qui mènent de SF à une SG
              if (ConsoliderSF(situation))
              {
                // Inscrire ces mouvements complémentaires dans la liste des mouvements de la solution incomplète
                for (int idxMvt = situation.NbPierres; idxMvt > 1; idxMvt--)
                {
                  var mvtSolution = Mouvements[idxMvt];
                  solution.Mouvements.Add(new SolutionMouvement((byte)mvtSolution.idxOrigine, (byte)mvtSolution.idxSaut));
                }
                solution.Complete = true;
                Config.SauvePilote();
              }
              else
              {
                Parent?.Feedback(enumFeedbackHint.error, "Impossible de retrouver les mouvements qui complètent une situation finale");
              }
            }
            if (State != enumState.running)
            {
              Parent?.Feedback(enumFeedbackHint.info, "Prise en compte de la demande d'arrêt");
              break;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Parent?.Feedback(enumFeedbackHint.error, $"{ex}");
      }
      State = enumState.stopped;
      Parent?.Feedback(enumFeedbackHint.endOfJob, "Fin du job");
    }

    private bool ConsoliderSF(Situation situation)
    {
      if (situation.NbPierres == 1)
      {
        return true;
      }

      SituationEtude situationEtude = SituationsEtude[situation.NbPierres];
      situationEtude.ChargeSituation(situation);
      foreach ((byte idxOrigine, byte idxVoisin, byte idxDestination) mvt in Plateau.MouvementsPossibles)
      {
        if (situationEtude.MouvementPossible(mvt))
        {
          situationEtude.EffectueMouvement(mvt);
          if (!SituationEtudieeExisteDeja(situationEtude))
          {
            Situation situationSuivante = situationEtude.NewSituation();
            SituationStock.Add(situationSuivante);
            Mouvements[situation.NbPierres] = ((int)mvt.idxOrigine, (int)mvt.idxVoisin, (int)mvt.idxDestination);
            if (ConsoliderSF(situationSuivante))
            {
              return true;
            }
          }
          situationEtude.EffectueMouvementInverse(mvt);
        }
      }

      return false;
    }

    private bool LitEntreeED(FileStream EDdat, byte[] dualSituationDepartRaw, out byte situationsInitialesAssociees, Plateau plateau)
    {
      int tailleDualSituationDepartRaw = dualSituationDepartRaw.Length;
      int n = EDdat.Read(dualSituationDepartRaw, 0, tailleDualSituationDepartRaw);

      if (n == 0)
      {
        situationsInitialesAssociees = 0;
        return false;
      }
      if (n != tailleDualSituationDepartRaw)
      {
        throw new ApplicationException("Erreur de lecture de ED.dat");
      }
      for (int idxDual = 0; idxDual < tailleDualSituationDepartRaw; idxDual++)
      {
        if (!plateau.Contains(dualSituationDepartRaw[idxDual]))
        {
          throw new ApplicationException("Erreur de lecture de ED.dat");
        }
      }
      n = EDdat.ReadByte();
      if (n == -1)
      {
        throw new ApplicationException("Erreur de lecture de ED.dat");
      }
      situationsInitialesAssociees = (byte)n;
      return true;
    }
    private List<Solution> Consolider(PreSolution preSolution)
    {
      List<Solution> solutions = new List<Solution>();

      int tailleDualSituationDepartRaw = Plateau.NbCasesPlateau - Config.TailleSituationsND;
      byte[] dualSituationDepartRaw = new byte[tailleDualSituationDepartRaw];
      int nbSituationsInitiales = EG.Count;
      byte situationsInitialesAssociees;
      ScheduleInfo.Start();
      Situation situationDepart;
      using (FileStream EDdat = Config.OpenData(enumAccesData.EDdat))
      {
        long offsetFichierED = preSolution.IdxSD * (tailleDualSituationDepartRaw + nbSituationsInitiales);
        if (offsetFichierED >= EDdat.Length)
        {
          Parent?.Feedback(enumFeedbackHint.error, "Impossible de retrouver la SD associée à une présolution.");
          return solutions;
        }
        EDdat.Seek(offsetFichierED, SeekOrigin.Begin);
        if (!LitEntreeED(EDdat, dualSituationDepartRaw, out situationsInitialesAssociees, Plateau))
        {
          return solutions;
        }
        byte[] situationDepartRaw = Plateau.SituationDualeRaw(dualSituationDepartRaw);
        situationDepart = new Situation(situationDepartRaw);
      }
      // Rechercher toutes les situations initiales qui mènent à situationDepart,
      // et surtout les mouvements qui peuvent y mener.
      Consolider(situationDepart, solutions);

      return solutions;
    }

    // Recherche de toutes les SI accessibles depuis cette situation
    // Ajouter une solution incomplète pour chacune des SI découverte qui n'a pas déjà de solution (incomplète ou non)
    private void Consolider(Situation situation, List<Solution> solutions)
    {
      SituationEtude situationEtude = SituationsEtude[situation.NbPierres];
      situationEtude.ChargeSituation(situation);
      foreach (var mvt in Plateau.MouvementsPossibles)
      {
        if (situationEtude.MouvementInversePossible(mvt))
        {
          situationEtude.EffectueMouvementInverse(mvt);
          if (!SituationEtudieeExisteDeja(situationEtude))
          {
            Situation situationPrevious = situationEtude.NewSituation();
            SituationStock.Add(situationPrevious);
            Mouvements[situation.NbPierres] = mvt;
            if (situationPrevious.NbPierres == 36)
            {
              // Cette situation initiale est-elle non résolue ?
              if (Consolider(situationPrevious))
              {
                Solution solution = new Solution();
                solutions.Add(solution);
                solution.SituationInitiale = situationPrevious;
                // Pas très optimisé ...
                solution.SituationInitialeRaw = Common.ChargeSituationRaw(situationPrevious.Dump(Plateau));
                for (int idxMvt = 37 - 1; idxMvt >= Config.TailleSituationsND; idxMvt--)
                {
                  var mvtSolution = Mouvements[idxMvt];
                  solution.Mouvements.Add(new SolutionMouvement((byte)mvtSolution.idxOrigine, (byte)mvtSolution.idxSaut));
                }
              }
            }
            else
            {
              Consolider(situationPrevious, solutions);
            }
          }
          situationEtude.EffectueMouvement(mvt);
        }
      }
    }

    private bool Consolider(Situation situationInitiale)
    {
      SituationEtude situationEtude = new SituationEtude(Plateau);
      situationEtude.ChargeSituation(situationInitiale);
      SituationInitiale situationInitialeEI = (SituationInitiale)TryGetSituation(situationEtude, new HashSet<SituationBase>(EG));
      // Dans le cas d'une fiche de jeu qui cible une situation initiale particulière...
      if (situationInitialeEI == null || situationInitialeEI.Resolue)
      {
        return false;
      }
      situationInitialeEI.Resolue = true;
      return true;
    }

  }
}
