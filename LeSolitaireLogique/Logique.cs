using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private IFeedback Parent;
    public enumState State { get; private set; } = enumState.stopped;
    public enumOp EnumOp { get; private set; }
    private Task BgTask;
    public LogiqueConfiguration Config;
    private Plateau Plateau;
    // Buffers permettant la manipulation d'une situation
    private SituationEtude[] SituationsEtude;
    // Suivi des situations déjà rencontrées
    public SituationStock SituationStock;
    // Liste des situations initiales, et leur suivi (résolues ou non)
    private List<SituationInitiale> EI;
    // Liste des situations gagnantes (utile uniquement pour construire EF.dat)
    private List<Situation> EG;
    // Petit utilitaire permettant d'émettre des infos régulières, de + en + espacées
    private ScheduleInfo ScheduleInfo = new ScheduleInfo();
    // tableau des mouvements effectués
    private (int idxOrigine, int idxSaut, int idxDestination)[] Mouvements;
    private string PlateauFinal;
    public Logique(IFeedback parent)
    {
      Parent = parent;
      Config = new LogiqueConfiguration();
    }
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
          EnumOp |= enumOp.ReglerNDNF | enumOp.Rechercher;
        }
        else
        {
          EnumOp &= ~enumOp.Rechercher;
        }
      }
      return EnumOp;
    }
    public void LanceRecherche(FileInfo file)
    {
      if (State != enumState.stopped)
      {
        Parent?.Feedback(enumFeedbackHint.error, "processus en cours");
        return;
      }
      Parent?.Feedback(enumFeedbackHint.info, "Début recherche");
      Config = new LogiqueConfiguration();
      Config.FicheDeJeu = new FileInfo(file.FullName);
      State = enumState.running;
      BgTask = new Task(Recherche);
      BgTask.Start();
    }
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

    void Recherche()
    {
      if (!InitialisationOld())
      {
        State = enumState.stopped;
        Parent?.Feedback(enumFeedbackHint.endOfJob, "Fin du job");
        return;
      }
      byte[] situationInitialeRaw = new byte[Config.TailleSituationsND];
      ScheduleInfo.Start();
      Mouvements = new (int idxOrigine, int idxSaut, int idxDestination)[Plateau.NbCasesPlateau];
      FileStream fileStream = Config.OpenData(enumAccesData.EDdat);
      while (State == enumState.running)
      {
        long idxReprise = Config.Pilote.IdxReprise;
        long offsetFichierED = idxReprise * Config.TailleSituationsND;
        if (offsetFichierED > Config.FileSize(enumAccesData.EDdat))
        {
          // todo : sauver la configuration pilote, en particulier son IdxReprise
          break;
        }
        fileStream.Seek(offsetFichierED, SeekOrigin.Begin);
        fileStream.Read(situationInitialeRaw, 0, Config.TailleSituationsND);
        Parent?.Feedback(enumFeedbackHint.info, $"Début recherche ED d'indice {idxReprise}");
        Situation situationInitiale = new Situation(situationInitialeRaw);
        if (Recherche(situationInitiale))
        {
          StringBuilder compteRendu = new StringBuilder();
          compteRendu.AppendLine().AppendLine($"{situationInitiale.Dump(Plateau)}");
          for (int idxMvt = Config.TailleSituationsND; idxMvt > Config.TailleSituationsNF; idxMvt--)
          {
            (int idxDepart, int idxSaut, int idxArrivee) mvt = Mouvements[idxMvt];
            compteRendu.AppendLine($"{mvt}");
          }
          compteRendu.Append(PlateauFinal);
          Parent?.Feedback(enumFeedbackHint.info, compteRendu.ToString());
        }
        else
        {
          Parent?.Feedback(enumFeedbackHint.info, $"Recherche infructueuse d'indice {idxReprise}");
        }
        Config.Pilote.IdxReprise++;
      }
      fileStream.Close();
      State = enumState.stopped;
      Parent?.Feedback(enumFeedbackHint.endOfJob, "Fin du job");
    }

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
    private void Initialiser(string filenameFicheDeJeu)
    {
      if (!Config.Initialiser(Parent, filenameFicheDeJeu))
      {
        State = enumState.stopped;
        Parent?.Feedback(enumFeedbackHint.endOfJob, "Fin du job");
        return;
      }
      Plateau = new Plateau(Config.Pilote.PlateauRaw);
      SituationStock = new SituationStock(Plateau.NbCasesPlateau);
      SituationsEtude = new SituationEtude[Plateau.NbCasesPlateau];
      for (int idxSituationEtude = 0; idxSituationEtude < Plateau.NbCasesPlateau; idxSituationEtude++)
      {
        SituationsEtude[idxSituationEtude] = new SituationEtude(Plateau);
      }
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
        Situation situationGagnante = EG[idxSG];
        SituationEtude situationEtude = SituationsEtude[situationGagnante.NbPierres];
        situationEtude.ChargeSituation(situationGagnante);
        foreach (var mvt in Plateau.MouvementsPossibles)
        {
          if (situationEtude.MouvementInversePossible(mvt))
          {
            situationEtude.EffectueMouvementInverse(mvt);
            Situation situationFinale = TryGetSituation(situationEtude, SFnew) as Situation;
            if (situationFinale == null)
            {
              situationFinale = situationEtude.NewSituation();
              SFnew.Add(situationFinale);
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

    private bool InitialisationOld()
    {
      return true;
    }

    private void CalculeSituationsInitiales()
    {
      EI = new List<SituationInitiale>();
      int nbCases = Config.Pilote.PlateauRaw.Count;
      int nbPierres = Config.Pilote.PlateauRaw.NbPierres;
      SituationEtude situationEtude = new SituationEtude(Plateau);
      if (nbCases == nbPierres)
      {
        HashSet<SituationBase> hEI = new HashSet<SituationBase>();
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
          if (TryGetSituation(situationEtude, hEI) == null)
          {
            SituationInitiale situationInitiale = new SituationInitiale(situationEtude.NewSituation());
            hEI.Add(situationInitiale);
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

    private bool ChargeEF()
    {
      Parent?.Feedback(enumFeedbackHint.info, "Début du chargement des situations finales");
      ScheduleInfo.Start();
      int cptSituationsLues = 0;
      FileStream fileStream = Config.OpenData(enumAccesData.EFdat);
      long sizeEF = fileStream.Length;
      if ((sizeEF % Config.TailleSituationsNF) != 0 || sizeEF == 0)
      {
        Parent?.Feedback(enumFeedbackHint.error, "La taille de EF.dat est incorrecte");
        fileStream.Close();
        return false;
      }
      byte[] situationRaw = new byte[Config.TailleSituationsNF];
      for (; ; )
      {
        int octetsLus = fileStream.Read(situationRaw, 0, Config.TailleSituationsNF);
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
      Parent?.Feedback(enumFeedbackHint.info, $"{cptSituationsLues} situations chargées");
      Parent?.Feedback(enumFeedbackHint.info, "Fin du chargement des situations finales");
      return true;
    }

    // Comparer les situations de EI avec celles de la liste des solutions déjà découvertes
    // Processus : les EI sont générées à l'initialisation
    // Puis lors de la recherche, une situation de ED matche une situation de EF (à une symétrie près)
    // On cherche alors les situations de EI (non encore résolues) qui matchent
    private void MarqueSituationsInitialesResolues()
    {
      foreach (Solution solution in Config.Pilote.Solutions)
      {
        Situation situationSolution = new Situation(Plateau.Etendue, solution.SituationInitialeRaw);
        solution.Situation = situationSolution;
      }
      SituationEtude situationEtude = new SituationEtude(Plateau);
      foreach (SituationInitiale situationInitiale in EI)
      {
        situationEtude.ChargeSituation(situationInitiale);
        for (int idxSymetrie = 0; idxSymetrie < Plateau.NbSymetries; idxSymetrie++)
        {
          Plateau.GenereSymetrie(situationEtude, idxSymetrie);
          foreach (Solution solution in Config.Pilote.Solutions)
          {
            if (situationEtude.Equals(solution.Situation))
            {
              situationInitiale.Resolue = true;
              break;
            }
          }
          if (situationInitiale.Resolue)
          {
            break;
          }
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
              PlateauFinal = $"{situationNew.Dump(Plateau)}";
              return true;
            }
          }
          situationEtude.EffectueMouvementInverse(mvt);
          if (State != enumState.running)
          {
            return false;
          }
          if (ScheduleInfo.DelivreInfo())
          {
            Parent?.Feedback(enumFeedbackHint.info, $"Recherche en cours");
          }
        }
      }
      return false;
    }

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
        for (int idxOld = 0; idxOld < nbSDold; idxOld++)
        {
          int n = EDold.Read(dualSituationDepartRaw, 0, tailleDualSituationDepartRaw);
          if (n != tailleDualSituationDepartRaw)
          {
            Parent?.Feedback(enumFeedbackHint.error, "Erreur de lecture de ED.dat");
            return false;
          }
          n = EDold.Read(situationsNewAssociees, 0, nbSituationsInitiales);
          if (n != nbSituationsInitiales)
          {
            Parent?.Feedback(enumFeedbackHint.error, "Erreur de lecture de ED.dat");
            return false;
          }
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
      if (nf < Config.Pilote.Nf)
      {
        // Si c'est nécessaire pour ED (à cause de son tableau de liens avec les SI)
        //, ça ne l'est pas vraiment pour EF.
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


  }
}
