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

namespace LeSolitaireLogique
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
    // Tableau d buffers permettant la manipulation d'une situation
    // On gère un tel tableau car on fait une recherche en profondeur récursive
    private SituationsEtude SituationsEtude;
    // Suivi des situations déjà rencontrées
    public SituationStock SituationStock;
    // Liste des situations initiales, et leur suivi (résolues ou non)
    private SituationsInitiales EI;
    // Liste des situations gagnantes (utile uniquement pour construire EF.dat)
    private List<Situation> EG;
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
        CalculeSituationsInitiales();
        // Vérification de la taille des fichiers ED.dat et EF.dat
        // Suppression si tailles incorrectes
        if (!Config.VerifieTailleEDEF(Parent, EI.Count))
        {
          EnumOp = enumOp.Initialiser;
        }
      }
      if (EnumOp != enumOp.Initialiser)
      {
        if (Config.Pilote.PreSolutions.Count + Config.Pilote.Solutions.Count < EI.Count)
        {
          EnumOp |= enumOp.Rechercher;
          if (Config.Pilote.PreSolutions.Count == 0)
          {
            EnumOp |= enumOp.ReglerNDNF | enumOp.ArrangerND;
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
          CalculeSituationsInitiales();
          CalculeSituationsGagnantes();

          if (!Config.Exist(enumAccesData.EDdat))
          {
            InitialiserED();
          }
          if (!Config.Exist(enumAccesData.EFdat))
          {
            InitialiserEF();
          }

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

    private void InitialiserED()
    {
      HashSet<SituationBase> SDnew = new HashSet<SituationBase>();
      SituationEtude situationEtude = new SituationEtude(Plateau);
      for (int idxSI = 0; idxSI < EI.Count; idxSI++)
      {
        SituationInitiale situationInitiale = EI[idxSI];
        situationEtude.ChargeSituation(situationInitiale);
        foreach (var mvt in Plateau.MouvementsPossibles)
        {
          if (situationEtude.MouvementPossible(mvt))
          {
            situationEtude.EffectueMouvement(mvt);
            SituationDepart situationDepart = TryGetSituation(situationEtude, SDnew) as SituationDepart;
            if (situationDepart == null)
            {
              Situation situationNew = situationEtude.NewSituation();
              situationDepart = new SituationDepart(situationNew.Pierres, EI.Count);
              SDnew.Add(situationDepart);
            }
            situationDepart.IdxSituationsInitiales[idxSI] = 0xFF;
            situationEtude.EffectueMouvementInverse(mvt);
          }
        }
      }
      FileStream fileStream = Config.OpenData(enumAccesData.EDtmp);
      foreach (SituationDepart situationDepart1 in SDnew)
      {
        byte[] situationDuale = Plateau.SituationDualeRaw(situationDepart1.Pierres);
        fileStream.Write(situationDuale, 0, situationDuale.Length);
        fileStream.Write(situationDepart1.IdxSituationsInitiales, 0, EI.Count);
      }
      fileStream.Close();
      Config.ReplaceData(enumAccesData.EDdat);
      Config.Pilote.ChangeND(1);
      Config.SauvePilote();
    }

    private void InitialiserEF()
    {
      HashSet<SituationBase> SFnew = new HashSet<SituationBase>();
      for (int idxSG = 0; idxSG < EG.Count; idxSG++)
      {
        Situation situationFinale = EG[idxSG];
        SituationEtude situationEtude = SituationsEtude[situationFinale.NbPierres];
        situationEtude.ChargeSituation(situationFinale);
        foreach (var mvt in Plateau.MouvementsPossibles)
        {
          if (situationEtude.MouvementInversePossible(mvt))
          {
            situationEtude.EffectueMouvementInverse(mvt);
            Situation situationFinaleNew = TryGetSituation(situationEtude, SFnew) as Situation;
            if (situationFinaleNew == null)
            {
              situationFinaleNew = situationEtude.NewSituation();
              SFnew.Add(situationFinaleNew);
            }
            situationEtude.EffectueMouvement(mvt);
          }
        }
      }
      FileStream fileStream = Config.OpenData(enumAccesData.EFtmp);
      foreach (Situation situationFinale1 in SFnew)
      {
        fileStream.Write(situationFinale1.Pierres, 0, situationFinale1.NbPierres);
      }
      fileStream.Close();
      Config.ReplaceData(enumAccesData.EFdat);
      Config.Pilote.ChangeNF(1);
      Config.SauvePilote();
    }

    // Lance la constrution du fichier ED.dat pour une valeur donnée du paramètre ND
    public void LanceReglerND(int nd)
    {
      if (State != enumState.stopped)
      {
        Parent?.Feedback(enumFeedbackHint.error, "processus en cours");
        return;
      }
      Parent?.Feedback(enumFeedbackHint.info, $"Début réglage de ND à la valeur {nd}");
      State = enumState.running;
      BgTask = Task.Run(() => ReglerND(nd));
    }

    private void ReglerND(int nd)
    {
      try
      {
        if (nd < Config.Pilote.Nd)
        {
          Parent?.Feedback(enumFeedbackHint.info, $"ND décroit, on doit réinitialiser ND à 1");
          InitialiserED();
        }
        ScheduleInfo.Start();
        while (nd > Config.Pilote.Nd && State == enumState.running)
        {
          Parent?.Feedback(enumFeedbackHint.info, $"incrémentation de ND de {Config.Pilote.Nd} à {Config.Pilote.Nd + 1}");
          if (!IncrementerND())
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

    private bool IncrementerND()
    {
      HashSet<SituationBase> SDnew = new HashSet<SituationBase>();
      SituationEtude situationEtude = new SituationEtude(Plateau);
      // En lecture
      using (FileStream EDold = Config.OpenData(enumAccesData.EDdat))
      {
        int tailleDualSituationDepartRaw = Plateau.NbCasesPlateau - Config.TailleSituationsND;
        byte[] dualSituationDepartRaw = new byte[tailleDualSituationDepartRaw];
        int nbSituationsInitiales = EI.Count;
        byte[] situationsNewAssociees = new byte[nbSituationsInitiales];
        long nbSDold = EDold.Length / (tailleDualSituationDepartRaw + nbSituationsInitiales);
        while (LitEntreeED(EDold, dualSituationDepartRaw, situationsNewAssociees, Plateau))
        {
          byte[] situationDepartRaw = Plateau.SituationDualeRaw(dualSituationDepartRaw);
          situationEtude.ChargeSituation(situationDepartRaw);
          foreach (var mvt in Plateau.MouvementsPossibles)
          {
            if (situationEtude.MouvementPossible(mvt))
            {
              situationEtude.EffectueMouvement(mvt);
              SituationDepart situationDepart = TryGetSituation(situationEtude, SDnew) as SituationDepart;
              if (situationDepart == null)
              {
                Situation situationNew = situationEtude.NewSituation();
                situationDepart = new SituationDepart(situationNew.Pierres, situationsNewAssociees);
                SDnew.Add(situationDepart);
              }
              else
              {
                for (int idxSituationInitiale = 0; idxSituationInitiale < nbSituationsInitiales; idxSituationInitiale++)
                {
                  if (situationsNewAssociees[idxSituationInitiale] == 0xFF)
                  {
                    situationDepart.IdxSituationsInitiales[idxSituationInitiale] = 0xFF;
                  }
                }
              }
              situationEtude.EffectueMouvementInverse(mvt);
            }
          }
          if (ScheduleInfo.DelivreInfo())
          {
            Parent?.Feedback(enumFeedbackHint.info, $"Calcul de {SDnew.Count} situations");
          }
          if (State != enumState.running)
          {
            Parent.Feedback(enumFeedbackHint.info, "Demande d'arrêt prise en compte");
            return false;
          }
        }
      }

      // En écriture
      using (FileStream EDnew = Config.OpenData(enumAccesData.EDtmp))
      {
        foreach (SituationDepart situationDepart1 in SDnew)
        {
          byte[] situationDuale = Plateau.SituationDualeRaw(situationDepart1.Pierres);
          EDnew.Write(situationDuale, 0, situationDuale.Length);
          EDnew.Write(situationDepart1.IdxSituationsInitiales, 0, EI.Count);
        }
        Parent?.Feedback(enumFeedbackHint.info, $"Enregistrement de {SDnew.Count} situations");
      }
      Config.ReplaceData(enumAccesData.EDdat);
      Config.Pilote.IncrementerND();
      Config.SauvePilote();
      Config.TailleSituationsND--;
      return true;
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
          NbSD = EDdat.Length / (Plateau.NbCasesPlateau - Config.TailleSituationsND + EI.Count);
        }

        Parent?.Feedback(enumFeedbackHint.info, $"Début recherche nd={Config.Pilote.Nd}, nf={Config.Pilote.Nf}, nb SD={NbSD} idxReprise={Config.Pilote.IdxReprise} ({100f * Config.Pilote.IdxReprise / NbSD} %)");
        int tailleDualSituationDepartRaw = Plateau.NbCasesPlateau - Config.TailleSituationsND;
        byte[] dualSituationDepartRaw = new byte[tailleDualSituationDepartRaw];
        int nbSituationsInitiales = EI.Count;
        byte[] situationsInitialesAssociees = new byte[nbSituationsInitiales];
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
              if (!LitEntreeED(EDdat, dualSituationDepartRaw, situationsInitialesAssociees, Plateau))
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
                  if (situationsInitialesAssociees[idxSituationInitiale] != 0)
                  {
                    if (!EI[idxSituationInitiale].Resolue)
                    {
                      preSolution.IdxSIlist.Add(idxSituationInitiale);
                      EI[idxSituationInitiale].Resolue = true;
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
      return !EI.list.Any(si => !si.Resolue);
    }

    // Lorsqu'on teste une SD, on veut s'assurer qu'elle peut être utile pour au moins une SI.
    // Or si toutes les SI qui mènent à cette SD sont déjà résolues, il est inutile d'étudier cette SD.
    // on renvoie un compte-rendu, une chaine de EI.Count digits :
    //   0 : la SI ne concerne pas cette SD
    //   1 : la SI concerne cette SD et n'est pas encore résolue
    //   2 : la SI concerne cette SD mais est déjà résolue
    private bool RechercheNouvelleSIpossible(byte[] situationsInitialesAssociees)
    {
      for (int idxSI = 0; idxSI < EI.Count; idxSI++)
      {
        if (situationsInitialesAssociees[idxSI] != 0)
        {
          if (!EI[idxSI].Resolue)
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
      CalculeSituationsInitiales();
      MarqueSituationsInitialesResolues(true);
      RechercheChargeEF();
      return true;
    }

    private void CalculeSituationsInitiales()
    {
      EI = new SituationsInitiales();
      int nbCases = Config.Pilote.PlateauRaw.Count;
      int nbPierres = Config.Pilote.PlateauRaw.NbPierres;
      SituationEtude situationEtude = new SituationEtude(Plateau);
      if (nbCases == nbPierres)
      {
        for (int idxCase = 0; idxCase < nbCases; idxCase++)
        {
          // Pour chaque case du plateau plein, on retire la pierre afin de constituer une situation initiale possible
          SituationRaw situationInitialeRaw = new SituationRaw();
          situationInitialeRaw.AddRange(Config.Pilote.PlateauRaw);
          // En fait, pour ce que l'on veut en faire, 
          // il n'est pas nécessaire de conserver dans la description une case si elle est vide.
          situationInitialeRaw.RemoveAt(idxCase);
          situationEtude.ChargeSituationRaw(situationInitialeRaw);
          // Si aucune symétrie de situationInitialeRaw n'est encore dans le stock, y ajouter situationInitialeRaw
          if (TryGetSituation(situationEtude, EI.hashset) == null)
          {
            SituationInitiale situationInitiale = new SituationInitiale(situationEtude.NewSituation());
            EI.Add(situationInitiale);
          }
        }
      }
      else
      {
        Situation situationInitiale = new Situation(Plateau.Etendue, Config.Pilote.PlateauRaw);
        EI.Add(new SituationInitiale(situationInitiale));
      }
    }

    private void CalculeSituationsGagnantes()
    {
      EG = new List<Situation>();
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
        // Si aucune symétrie de situationInitialeRaw n'est encore dans le stock, y ajouter situationInitialeRaw
        if (TryGetSituation(situationEtude, hEG) == null)
        {
          Situation situationGagnante = situationEtude.NewSituation();
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
            EI[idxPreSolution].Resolue = true;
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
      foreach (SituationInitiale situationInitiale in EI.list.FindAll(si => !si.Resolue))
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
        CalculeSituationsInitiales();
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

    private bool LitEntreeED(FileStream EDdat, byte[] dualSituationDepartRaw, byte[] situationsInitialesAssociees, Plateau plateau)
    {
      int tailleDualSituationDepartRaw = dualSituationDepartRaw.Length;
      int nbSituationsInitiales = situationsInitialesAssociees.Length;
      int n = EDdat.Read(dualSituationDepartRaw, 0, tailleDualSituationDepartRaw);
      if (n == 0)
      {
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
      n = EDdat.Read(situationsInitialesAssociees, 0, nbSituationsInitiales);
      if (n != nbSituationsInitiales)
      {
        throw new ApplicationException("Erreur de lecture de ED.dat");
      }
      for (int idxSI = 0; idxSI < nbSituationsInitiales; idxSI++)
      {
        if (situationsInitialesAssociees[idxSI] != 0 && situationsInitialesAssociees[idxSI] != 0xFF)
        {
          throw new ApplicationException("Erreur de lecture de ED.dat");
        }
      }
      return true;
    }
    private List<Solution> Consolider(PreSolution preSolution)
    {
      List<Solution> solutions = new List<Solution>();

      int tailleDualSituationDepartRaw = Plateau.NbCasesPlateau - Config.TailleSituationsND;
      byte[] dualSituationDepartRaw = new byte[tailleDualSituationDepartRaw];
      int nbSituationsInitiales = EI.Count;
      byte[] situationsInitialesAssociees = new byte[nbSituationsInitiales];
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
        if (!LitEntreeED(EDdat, dualSituationDepartRaw, situationsInitialesAssociees, Plateau))
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
            if (situationPrevious.NbPierres == EI.NbPierres)
            {
              // Cette situation initiale est-elle non résolue ?
              if (Consolider(situationPrevious))
              {
                Solution solution = new Solution();
                solutions.Add(solution);
                solution.SituationInitiale = situationPrevious;
                // Pas très optimisé ...
                solution.SituationInitialeRaw = Common.ChargeSituationRaw(situationPrevious.Dump(Plateau));
                for (int idxMvt = EI.NbPierres - 1; idxMvt >= Config.TailleSituationsND; idxMvt--)
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
      SituationInitiale situationInitialeEI = (SituationInitiale)TryGetSituation(situationEtude, EI.hashset);
      // Dans le cas d'une fiche de jeu qui cible une situation initiale particulière...
      if (situationInitialeEI == null || situationInitialeEI.Resolue)
      {
        return false;
      }
      situationInitialeEI.Resolue = true;
      return true;
    }

    public void LanceArrangerED()
    {
      if (State != enumState.stopped)
      {
        Parent?.Feedback(enumFeedbackHint.error, "processus en cours");
        return;
      }
      State = enumState.running;
      BgTask = new Task(ArrangerED);
      BgTask.Start();
    }

    private void ArrangerED()
    {
      try
      {
        // Initialisations
        SituationEtude situationEtude = new SituationEtude(Plateau);
        HashSet<SituationBase> stock = new HashSet<SituationBase>();
        // Chargement des situations initiales
        foreach (Solution solution in Config.Pilote.Solutions)
        {
          Situation situation = new Situation(Plateau.Etendue, solution.SituationInitialeRaw);
          solution.SituationInitiale = new SituationInitiale(situation);
          stock.Add(solution.SituationInitiale);
        }
        foreach (SituationInitiale item in EI.list)
        {
          situationEtude.ChargeSituation(item);
          SituationInitiale situationInitiale = TryGetSituation(situationEtude, stock) as SituationInitiale;
          if (situationInitiale != null)
          {
            item.Resolue = true;
          }
        }
        int tailleDualSituationDepartRaw = Plateau.NbCasesPlateau - Config.TailleSituationsND;
        byte[] dualSituationDepartRaw = new byte[tailleDualSituationDepartRaw];
        int nbSituationsInitiales = EI.Count;
        byte[] situationsInitialesAssociees = new byte[nbSituationsInitiales];
        Dictionary<string, HashSet<SituationDepart>> dict = new Dictionary<string, HashSet<SituationDepart>>();
        using (FileStream EDdat = Config.OpenData(enumAccesData.EDdat))
        {
          EDdat.Seek(Config.Pilote.IdxReprise * (tailleDualSituationDepartRaw + nbSituationsInitiales), SeekOrigin.Begin);
          while (State == enumState.running)
          {
            if (!LitEntreeED(EDdat, dualSituationDepartRaw, situationsInitialesAssociees, Plateau))
            {
              break;
            }
            string signature = string.Empty;
            // Pour chaque SI, on ajoute dans la signature :
            //  un '1' si cette SI est incluse dans les SI de la SD et si elle n'est pas encore résolue
            //  un '1' dans le cas contraire
            for (int idxSI = 0; idxSI < nbSituationsInitiales; idxSI++)
            {
              bool inclure = situationsInitialesAssociees[idxSI] != 0 && !EI.list[idxSI].Resolue;
              if (inclure)
              {
                signature += '1';
              }
              else
              {
                situationsInitialesAssociees[idxSI] = 0;
                signature += ' ';
              }
            }
            if (signature.Count(c => c == '1') != 0)
            {
              if (!dict.ContainsKey(signature))
              {
                dict.Add(signature, new HashSet<SituationDepart>());
              }
              byte[] situationDepartRaw = Plateau.SituationDualeRaw(dualSituationDepartRaw);
              SituationDepart situationDepart = new SituationDepart(situationDepartRaw, situationsInitialesAssociees);
              dict[signature].Add(situationDepart);
            }
          }
        }
        List<string> signatures = dict.Keys.ToList();
        // On privilégie les SD ayant le plus de SI associées
        signatures.Sort((x, y) => y.Count(c => c == '1').CompareTo(x.Count(c => c == '1')));
        if (State == enumState.running)
        {
          using (FileStream EDtmp = Config.OpenData(enumAccesData.EDtmp))
          {
            foreach (string signature in signatures)
            {
              HashSet<SituationDepart> situationsDepart = dict[signature];
              foreach (SituationDepart situationDepart in situationsDepart)
              {
                dualSituationDepartRaw = Plateau.SituationDualeRaw(situationDepart.Pierres);
                situationsInitialesAssociees = situationDepart.IdxSituationsInitiales;
                EDtmp.Write(dualSituationDepartRaw, 0, dualSituationDepartRaw.Length);
                EDtmp.Write(situationsInitialesAssociees, 0, situationsInitialesAssociees.Length);
              }
            }
          }
        }
        if (State == enumState.running)
        {
          Config.ReplaceData(enumAccesData.EDdat);
          Config.Pilote.IdxReprise = 0;
          Config.SauvePilote();
        }
      }
      catch (Exception ex)
      {
        Parent.Feedback(enumFeedbackHint.error, ex.ToString());
      }
      State = enumState.stopped;
      Parent?.Feedback(enumFeedbackHint.endOfJob, "Fin du job");
    }
  }
}
