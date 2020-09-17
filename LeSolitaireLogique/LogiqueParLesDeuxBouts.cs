using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class LogiqueParLesDeuxBouts
  {
    private IFeedback Parent;
    private enumState State = enumState.stopped;
    private Task RechercheTask;
    private class ConfigurationClass
    {
      // Le fichier fourni, ne contient que le plateau initial
      public FileInfo FichierInitialisation;
      // Le répertoire qui porte le nom du fichier FichierInitialisation et dans lequel on stocke les données de la recherche
      public DirectoryInfo RacineData;
      // Le contenu du fichier FichierInitialisation
      public ParLesDeuxBoutsConfig ConfigInitialisation;
      // Le fichier pilote
      public FileInfo FichierPilote;
      // Le contenu du fichier FichierPilote
      public ParLesDeuxBoutsConfig ConfigPilote;
      // La taille des situations dans le fichier ED
      public int TailleSituationsND;
      // Le fichier ED contenant les situations possibles après les ND premiers mouvements
      public FileInfo FichierED;
      // La taille des situations dans le fichier EF
      public int TailleSituationsNF;
      // Le fichier EF contenant les situations possibles qui mènent à une solution en NF mouvements
      public FileInfo FichierEF;
      // L'acces aux situations intermédiaires
      public FileStream StreamED;
      // L'acces aux situations gagnantes
      public FileStream StreamEF;
      // On stocke initialement 
      // La ou les situations de départ (à une seule case vide, ou celle fournie)
      // les situations ED (à 'Initial.NbPierres - ND) pierres
      // les situations EF (à 1 + NF pierres)
      // les situations gagnantes (à une seule pierre)
      public SituationsStock SituationsStock;
    }
    private ConfigurationClass Configuration = new ConfigurationClass();
    public LogiqueParLesDeuxBouts(IFeedback parent)
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
      while (State != enumState.stopping)
      {
        Thread.Sleep(500);
        Parent?.Feedback(enumFeedbackHint.trace, "ça bosse");
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
        Configuration.ConfigInitialisation = new ParLesDeuxBoutsConfig(Configuration.FichierInitialisation);
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
      int nbCases = Configuration.ConfigInitialisation.Plateau.Count;
      int nbPierres = Configuration.ConfigInitialisation.Plateau.NbPierres;
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
      bool bOK;
      if (Configuration.RacineData.Exists)
      {
        bOK = VerifieData();
      }
      else
      {
        bOK = InitData();
      }
      return bOK;
    }

    private bool InitData()
    {
      try
      {
        Configuration.RacineData.Create();
        File.Copy(Configuration.FichierInitialisation.FullName, Configuration.FichierPilote.FullName);
        Configuration.ConfigPilote = new ParLesDeuxBoutsConfig(Configuration.FichierPilote);
        Configuration.ConfigPilote.SauveConfig();
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
      Configuration.SituationsStock = new SituationsStock(Configuration.ConfigInitialisation.Plateau.Count);
      CalculeSituationsInitiales();
      return State == enumState.running;
    }

    private void CalculeSituationsInitiales()
    {
      int nbCases = Configuration.ConfigInitialisation.Plateau.Count;
      int nbPierres = Configuration.ConfigInitialisation.Plateau.NbPierres;
      List<SituationPacked> SituationsPackedInitiales = new List<SituationPacked>();
      if (nbCases == nbPierres)
      {

      }
      else
      {
        SituationPacked situationPacked = new SituationPacked(nbPierres);
        SituationRaw plateau = Configuration.ConfigInitialisation.Plateau;
        Etendue etendue = plateau.Etendue;
        plateau.FindAll(c=>c.pierre).ForEach(c => situationPacked.Add(etendue.FromXY(c.x, c.y)));
      }
      throw new NotImplementedException();
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
        Configuration.ConfigPilote = new ParLesDeuxBoutsConfig(Configuration.FichierPilote);
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
      int l = Configuration.ConfigInitialisation.Plateau.Count;
      if (l != Configuration.ConfigPilote.Plateau.Count)
      {
        Parent?.Feedback(enumFeedbackHint.error, $"Les tailles du plateau initial diffèrent");
        return false;
      }
      for (int i = 0; i < l; i++)
      {
        (int x, int y, bool pierre) p1 = Configuration.ConfigInitialisation.Plateau[i], p2 = Configuration.ConfigPilote.Plateau[i];
        if (p1.x != p2.x || p1.y != p2.y || p1.pierre != p2.pierre)
        {
          Parent?.Feedback(enumFeedbackHint.error, $"Les contenus du plateau initial diffèrent");
          return false;
        }
      }
      return true;
    }
  }
}
