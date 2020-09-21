using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LeSolitaireLogique
{
  public class Logique
  {
    private IFeedback Parent;
    private enumState State = enumState.stopped;
    private Task RechercheTask;
    private LogiqueConfiguration Configuration;
    private Plateau Plateau;
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
    }
    public void LanceRecherche(FileInfo file)
    {
      if (State != enumState.stopped)
      {
        Parent?.Feedback(enumFeedbackHint.error, "processus en cours");
        return;
      }
      Parent?.Feedback(enumFeedbackHint.info, "Début recherche");
      Configuration = new LogiqueConfiguration();
      Configuration.FichierInitialisation = new FileInfo(file.FullName);
      State = enumState.running;
      RechercheTask = new Task(Recherche);
      RechercheTask.Start();
    }
    public void StoppeRecherche()
    {
      if (State != enumState.running)
      {
        Parent?.Feedback(enumFeedbackHint.error, "processus a l'arrêt ou en cours d'arrêt");
        return;
      }
      State = enumState.stopping;
    }

    void Recherche()
    {
      if (!Initialisation())
      {
        State = enumState.stopped;
        Parent?.Feedback(enumFeedbackHint.endOfJob, "Fin du job");
        return;
      }
      byte[] situationInitialeRaw = new byte[Configuration.TailleSituationsND];
      ScheduleInfo.Start();
      Mouvements = new (int idxOrigine, int idxSaut, int idxDestination)[Plateau.NbCasesPlateau];
      while (State == enumState.running)
      {
        long idxReprise = Configuration.ConfigInitialisation.IdxReprise;
        long offsetFichierED = idxReprise * Configuration.TailleSituationsND;
        if (offsetFichierED > Configuration.StreamED.Length)
        {
          // todo : sauver la configuration pilote, en particulier son IdxReprise
          break;
        }
        Configuration.StreamED.Seek(offsetFichierED, SeekOrigin.Begin);
        Configuration.StreamED.Read(situationInitialeRaw, 0, Configuration.TailleSituationsND);
        Parent?.Feedback(enumFeedbackHint.info, $"Début recherche ED d'indice {idxReprise}");
        Situation situationInitiale = new Situation(situationInitialeRaw);
        if (Recherche(situationInitiale))
        {
          StringBuilder compteRendu = new StringBuilder();
          compteRendu.AppendLine().AppendLine($"{situationInitiale.Dump(Plateau)}");
          for (int idxMvt = Configuration.TailleSituationsND; idxMvt > Configuration.TailleSituationsNF; idxMvt--)
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
        Configuration.ConfigInitialisation.IdxReprise++;
      }
      State = enumState.stopped;
      Parent?.Feedback(enumFeedbackHint.endOfJob, "Fin du job");
    }

    // Soit le répertoire de données n'existe pas et il est construit
    // Soit il existe et il est vérifié
    private bool Initialisation()
    {
      try
      {
        Configuration.ConfigInitialisation = new Config(Configuration.FichierInitialisation);
      }
      catch (Exception ex)
      {
        Parent?.Feedback(enumFeedbackHint.error, $"Impossible de décoder le contenu du fichier fourni : {ex.Message} ");
        return false;
      }

      string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Configuration.FichierInitialisation.Name);
      Configuration.RacineData = new DirectoryInfo(Path.Combine(Configuration.FichierInitialisation.DirectoryName, fileNameWithoutExtension));
      Configuration.FichierPilote = new FileInfo(Path.Combine(Configuration.RacineData.FullName, Configuration.FichierInitialisation.Name));
      Configuration.FichierED = new FileInfo(Path.Combine(Configuration.RacineData.FullName, "ED.dat"));
      int nbCases = Configuration.ConfigInitialisation.PlateauRaw.Count;
      int nbPierres = Configuration.ConfigInitialisation.PlateauRaw.NbPierres;
      if (nbCases == nbPierres)
      {
        Configuration.TailleSituationsND = nbPierres - 1 - Configuration.ConfigInitialisation.Nd;
      }
      else
      {
        Configuration.TailleSituationsND = nbPierres - Configuration.ConfigInitialisation.Nd;
      }
      Configuration.TailleSituationsNF = 1 + Configuration.ConfigInitialisation.Nf;

      Configuration.FichierEF = new FileInfo(Path.Combine(Configuration.RacineData.FullName, "EF.dat"));

      Plateau = new Plateau(Configuration.ConfigInitialisation.PlateauRaw);
      SituationsEtude = new SituationEtude[Plateau.NbCasesPlateau];
      for (int idxSituationEtude = 0; idxSituationEtude < Plateau.NbCasesPlateau; idxSituationEtude++)
      {
        SituationsEtude[idxSituationEtude] = new SituationEtude(Plateau);
      }
      SituationStock = new SituationStock(Configuration.ConfigInitialisation.PlateauRaw.Count);
      // A  faire ici, sera utile si on doit construire ED.dat et EF.dat
      CalculeSituationsInitiales();
      CalculeSituationsGagnantes();

      bool bOK;
      if (Configuration.RacineData.Exists)
      {
        bOK = VerifieData();
      }
      else
      {
        bOK = InitData();
        if (!bOK)
        {
          Parent?.Feedback(enumFeedbackHint.info, $"Pensez à supprimer le répertoire {Configuration.RacineData.FullName}");
        }
      }
      if (bOK)
      {
        MarqueSituationsInitialesResolues();
        SituationStock = new SituationStock(Configuration.ConfigInitialisation.PlateauRaw.Count);
        GC.Collect();
        Configuration.StreamED = new FileStream(Configuration.FichierED.FullName, FileMode.Open, FileAccess.Read);
        Configuration.StreamEF = new FileStream(Configuration.FichierEF.FullName, FileMode.Open, FileAccess.Read);
        bOK = ChargeEF();
        Configuration.StreamEF.Close();
      }
      return bOK;
    }

    private void CalculeSituationsInitiales()
    {
      EI = new List<SituationInitiale>();
      int nbCases = Configuration.ConfigInitialisation.PlateauRaw.Count;
      int nbPierres = Configuration.ConfigInitialisation.PlateauRaw.NbPierres;
      if (nbCases == nbPierres)
      {
        for (int idxCase = 0; idxCase < nbCases; idxCase++)
        {
          // Pour chaque case du plateau plein, on retire la pierre afin de constituer une situation initiale possible
          SituationRaw situationInitialeRaw = new SituationRaw();
          situationInitialeRaw.AddRange(Configuration.ConfigInitialisation.PlateauRaw);
          // En fait, pour ce que l'on veut en faire, 
          // il n'est pas nécessaire de conserver dans la description une case si elle est vide.
          situationInitialeRaw.RemoveAt(idxCase);
          SituationEtude situationEtude = SituationsEtude[nbPierres - 1];
          situationEtude.ChargeSituationRaw(situationInitialeRaw);
          // Si aucune symétrie de situationInitialeRaw n'est encore dans le stock, y ajouter situationInitialeRaw
          if (!SituationEtudieeExisteDeja(situationEtude))
          {
            Situation situationInitiale = situationEtude.NewSituation();
            SituationStock.Add(situationInitiale);
            EI.Add(new SituationInitiale(situationInitiale));
          }
        }
      }
      else
      {
        Situation situationInitiale = new Situation(Plateau.Etendue, Configuration.ConfigInitialisation.PlateauRaw);
        SituationStock.Add(situationInitiale);
        EI.Add(new SituationInitiale(situationInitiale));
      }
    }

    private void CalculeSituationsGagnantes()
    {
      EG = new List<Situation>();
      int nbCases = Configuration.ConfigInitialisation.PlateauRaw.Count;
      foreach (byte idxCase in Plateau.Cases)
      {
        // Pour chaque case du plateau vide, on place la pierre afin de constituer une situation gagnante possible
        SituationRaw situationInitialeRaw = new SituationRaw();
        (int x, int y) coordonnee = Plateau.Etendue.FromByte(idxCase);
        // En fait, pour ce que l'on veut en faire, 
        // il n'est pas nécessaire de conserver dans la description une case si elle est vide.
        situationInitialeRaw.Add((coordonnee.x, coordonnee.y, true));
        SituationEtude situationEtude = SituationsEtude[1];
        situationEtude.ChargeSituationRaw(situationInitialeRaw);
        // Si aucune symétrie de situationInitialeRaw n'est encore dans le stock, y ajouter situationInitialeRaw
        if (!SituationEtudieeExisteDeja(situationEtude))
        {
          Situation situationGagnante = situationEtude.NewSituation();
          SituationStock.Add(situationGagnante);
          EG.Add(situationGagnante);
        }
      }
    }

    private bool InitData()
    {
      Parent?.Feedback(enumFeedbackHint.info, "Début de l'initialisation de la base de données");
      try
      {
        Configuration.RacineData.Create();
        File.Copy(Configuration.FichierInitialisation.FullName, Configuration.FichierPilote.FullName);
        Configuration.ConfigPilote = new Config(Configuration.FichierPilote);
        Configuration.ConfigPilote.SauveConfig().Save(Configuration.FichierPilote.FullName);
      }
      catch (Exception ex)
      {
        Parent?.Feedback(enumFeedbackHint.error, $"Impossible d'initialiser les données de recherche : {ex.Message}");
        return false;
      }
      try
      {
        Configuration.StreamED = new FileStream(Configuration.FichierED.FullName, FileMode.CreateNew, FileAccess.Write);
        Configuration.StreamEF = new FileStream(Configuration.FichierEF.FullName, FileMode.CreateNew, FileAccess.Write);
      }
      catch (Exception ex)
      {
        Parent?.Feedback(enumFeedbackHint.error, $"Impossible de créer les fichiers de données : {ex.Message}");
        return false;
      }
      FillED();
      if (State == enumState.running)
      {
        FillEF();
      }
      return State == enumState.running;
    }

    private bool VerifieData()
    {
      if (!Configuration.FichierPilote.Exists)
      {
        Parent?.Feedback(enumFeedbackHint.error, $"Impossible de trouver le fichier pilote dans le répertoire de données");
        return false;
      }
      try
      {
        Configuration.ConfigPilote = new Config(Configuration.FichierPilote);
      }
      catch (Exception ex)
      {
        Parent?.Feedback(enumFeedbackHint.error, $"Impossible de décoder le contenu du fichier pilote : {ex.Message} ");
        return false;
      }
      if (!VerifieConsistenceConfig())
        return false;

      if (!Configuration.FichierED.Exists)
      {
        Parent?.Feedback(enumFeedbackHint.error, $"Impossible de trouver les situations initiales {Configuration.FichierED.Name} ");
        return false;
      }
      if (!Configuration.FichierEF.Exists)
      {
        Parent?.Feedback(enumFeedbackHint.error, $"Impossible de trouver les situations finales {Configuration.FichierEF.Name} ");
        return false;
      }
      return true;
    }

    private bool VerifieConsistenceConfig()
    {
      if (Configuration.ConfigInitialisation.Nd != Configuration.ConfigPilote.Nd ||
          Configuration.ConfigInitialisation.Nf != Configuration.ConfigPilote.Nf)
      {
        Parent?.Feedback(enumFeedbackHint.error, $"Les parametres Nd ou Nf diffèrent");
        return false;
      }
      int l = Configuration.ConfigInitialisation.PlateauRaw.Count;
      if (l != Configuration.ConfigPilote.PlateauRaw.Count)
      {
        Parent?.Feedback(enumFeedbackHint.error, $"Les tailles du plateau initial diffèrent");
        return false;
      }
      for (int i = 0; i < l; i++)
      {
        (int x, int y, bool pierre) p1 = Configuration.ConfigInitialisation.PlateauRaw[i], p2 = Configuration.ConfigPilote.PlateauRaw[i];
        if (p1.x != p2.x || p1.y != p2.y || p1.pierre != p2.pierre)
        {
          Parent?.Feedback(enumFeedbackHint.error, $"Les contenus du plateau initial diffèrent");
          return false;
        }
      }
      return true;
    }

    private bool SituationEtudieeExisteDeja(SituationEtude situationEtude)
    {
      bool bExiste = false;
      for (int idxSymetrie = 0; idxSymetrie < Plateau.NbSymetries; idxSymetrie++)
      {
        Plateau.GenereSymetrie(situationEtude, idxSymetrie);
        //Debug.Print($"Symétrie {idxSymetrie}");
        //Debug.Print($"{situationEtude.DumpImagePierres(Plateau)}");
        if (SituationStock.Contains(situationEtude))
        {
          bExiste = true;
          break;
        }
      }
      return bExiste;
    }

    private void FillED()
    {
      Parent?.Feedback(enumFeedbackHint.info, "Début de l'initialisation des situations de départ");
      ScheduleInfo.Start();
      foreach (SituationInitiale situationInitiale in EI)
      {
        FillED(situationInitiale.Situation);
        if (State != enumState.running)
        {
          return;
        }
      }
      Parent?.Feedback(enumFeedbackHint.info, $"{Configuration.StreamED.Length / Configuration.TailleSituationsND} situations trouvées");
      // On va le rouvrir en lecture
      Configuration.StreamED.Close();
      Parent?.Feedback(enumFeedbackHint.info, "Fin de l'initialisation des situations de départ");
    }

    private void FillED(Situation situation)
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
            Situation situationNew = situationEtude.NewSituation();
            SituationStock.Add(situationNew);
            if (situationNew.NbPierres > Configuration.TailleSituationsND)
            {
              FillED(situationNew);
            }
            else
            {
              Configuration.StreamED.Write(situationNew.Pierres, 0, situationNew.NbPierres);
            }
            if (State != enumState.running)
            {
              return;
            }
            if (ScheduleInfo.DelivreInfo())
            {
              Parent?.Feedback(enumFeedbackHint.info, $"{Configuration.StreamED.Length / Configuration.TailleSituationsND} situations trouvées");
            }
          }
          situationEtude.EffectueMouvementInverse(mvt);
        }
      }
    }

    private void FillEF()
    {
      Parent?.Feedback(enumFeedbackHint.info, "Début de l'initialisation des situations finales");
      ScheduleInfo.Start();
      foreach (Situation situationGagnante in EG)
      {
        FillEF(situationGagnante);
        if (State != enumState.running)
        {
          return;
        }
      }
      Parent?.Feedback(enumFeedbackHint.info, $"{Configuration.StreamEF.Length / Configuration.TailleSituationsNF} situations trouvées");
      // On va le rouvrir en lecture
      Configuration.StreamEF.Close();
      Parent?.Feedback(enumFeedbackHint.info, "Fin de l'initialisation des situations finales");
    }

    private void FillEF(Situation situation)
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
            Situation situationNew = situationEtude.NewSituation();
            SituationStock.Add(situationNew);
            if (situationNew.NbPierres < Configuration.TailleSituationsNF)
            {
              FillEF(situationNew);
            }
            else
            {
              Configuration.StreamEF.Write(situationNew.Pierres, 0, situationNew.NbPierres);
              Debug.Print($"{Configuration.StreamEF.Length / Configuration.TailleSituationsNF}");
              Debug.Print($"{situationNew.Dump(Plateau)}");
            }
            if (State != enumState.running)
            {
              return;
            }
            if (ScheduleInfo.DelivreInfo())
            {
              Parent?.Feedback(enumFeedbackHint.info, $"{Configuration.StreamEF.Length / Configuration.TailleSituationsNF} situations trouvées");
            }
          }
          situationEtude.EffectueMouvement(mvt);
        }
      }
    }

    private bool ChargeEF()
    {
      Parent?.Feedback(enumFeedbackHint.info, "Début du chargement des situations finales");
      ScheduleInfo.Start();
      int cptSituationsLues = 0;
      long sizeEF = Configuration.StreamEF.Length;
      if ((sizeEF % Configuration.TailleSituationsNF) != 0)
      {
        Parent?.Feedback(enumFeedbackHint.error, $"La taille de {Configuration.FichierEF.FullName} n'est pas un multiple de {Configuration.TailleSituationsNF}");
        return false;
      }
      byte[] situationRaw = new byte[Configuration.TailleSituationsNF];
      for (; ; )
      {
        int octetsLus = Configuration.StreamEF.Read(situationRaw, 0, Configuration.TailleSituationsNF);
        if (octetsLus == 0)
        {
          break;
        }
        if (octetsLus != Configuration.TailleSituationsNF)
        {
          Parent?.Feedback(enumFeedbackHint.error, $"La taille de {Configuration.FichierEF.FullName} n'est pas un multiple de {Configuration.TailleSituationsNF}");
          return false;
        }
        foreach (byte idxCase in situationRaw)
        {
          if (!Plateau.Contains(idxCase))
          {
            Parent?.Feedback(enumFeedbackHint.error, $"Les données de {Configuration.FichierEF.FullName} sont incohérentes.");
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
      foreach (Solution solution in Configuration.ConfigPilote.Solutions)
      {
        Situation situationSolution = new Situation(Plateau.Etendue, solution.SituationInitialeRaw);
        solution.Situation = situationSolution;
      }
      SituationEtude situationEtude = new SituationEtude(Plateau);
      foreach (SituationInitiale situationInitiale in EI)
      {
        Situation situation = situationInitiale.Situation;
        situationEtude.ChargeSituation(situation);
        for (int idxSymetrie = 0; idxSymetrie < Plateau.NbSymetries; idxSymetrie++)
        {
          Plateau.GenereSymetrie(situationEtude, idxSymetrie);
          foreach (Solution solution in Configuration.ConfigPilote.Solutions)
          {
            if (situationEtude.Equals(solution.Situation))
            {
              situationInitiale.Situation = situationEtude.NewSituation();
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
            if (situationEtude.NbPierres > Configuration.TailleSituationsNF)
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
            if (situationEtude.NbPierres == Configuration.TailleSituationsNF)
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


  }
}
