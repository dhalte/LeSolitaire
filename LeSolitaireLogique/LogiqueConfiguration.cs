using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LeSolitaireLogique
{
  public class LogiqueConfiguration
  {
    // Le fichier fourni, ne contient que le plateau initial
    public FileInfo FicheDeJeu;
    // Le répertoire qui porte le nom de FicheDeJeu et dans lequel on stocke les données de la recherche
    public DirectoryInfo RacineData;
    // Le fichier pilote dans le répertoire
    public FileInfo FichierPilote;
    // Le contenu du fichier FichierPilote
    public Pilote Pilote;
    // La taille des situations dans le fichier ED
    public int TailleSituationsND;
    // La taille des situations dans le fichier EF
    public int TailleSituationsNF;

    internal enumOp Verifie(IFeedback parent, string filenameFicheDeJeu)
    {
      // Existence de la fiche de jeu
      if (string.IsNullOrEmpty(filenameFicheDeJeu))
      {
        throw new ApplicationException("Veuillez spécifier le chemin et le nom de la fiche de jeu");
      }
      if (!File.Exists(filenameFicheDeJeu))
      {
        throw new ApplicationException("Impossible de trouver la fiche de jeu spécifiée");
      }
      // Si un pb est rencontré, on passe par une initialisation, qui peut aussi corriger certaines erreurs
      enumOp enumOp = enumOp.Initialiser;
      FicheDeJeu = new FileInfo(filenameFicheDeJeu);
      // Controle du répertoire de données
      string filename = Path.GetFileNameWithoutExtension(FicheDeJeu.Name);
      string directoryName = Path.Combine(FicheDeJeu.DirectoryName, filename);
      RacineData = new DirectoryInfo(directoryName);
      if (!RacineData.Exists)
      {
        return enumOp;
      }
      // Controle de l'existence du pilote
      FichierPilote = new FileInfo(Path.Combine(RacineData.FullName, filename + ".xml"));
      if (!FichierPilote.Exists)
      {
        parent?.Feedback(enumFeedbackHint.error, "Il manque le fichier pilote.");
        return enumOp;
      }
      // Chargement du contenu du pilote
      try
      {
        Pilote = new Pilote(FichierPilote);
      }
      catch (Exception ex)
      {
        parent?.Feedback(enumFeedbackHint.error, $"Fichier pilote erroné : {ex.Message}");
        return enumOp;
      }
      // Controle de la cohérence du plateau initial dans la fiche de jeu et le pilote
      string descriptionPlateauInitial = File.ReadAllText(filenameFicheDeJeu);
      if (!Pilote.PlateauRaw.ControleDescription(descriptionPlateauInitial))
      {
        parent?.Feedback(enumFeedbackHint.error, "Incohérence de description du plateau initial avec le plateau pilote.");
        return enumOp;
      }

      // Controle de l'existence des fichiers de données
      if (!File.Exists(Path.Combine(RacineData.FullName, "ED.dat")))
      {
        parent?.Feedback(enumFeedbackHint.error, "Il manque le fichier ED.dat.");
        return enumOp;
      }
      if (!File.Exists(Path.Combine(RacineData.FullName, "EF.dat")))
      {
        parent?.Feedback(enumFeedbackHint.error, "Il manque le fichier EF.dat.");
        return enumOp;
      }
      int nbCases = Pilote.PlateauRaw.Count;
      int nbPierres = Pilote.PlateauRaw.NbPierres;
      if (nbCases == nbPierres) nbPierres--;
      TailleSituationsND = nbPierres - Pilote.Nd;
      TailleSituationsNF = 1 + Pilote.Nf;
      if (Pilote.PreSolutions.Count > 0)
      {
        enumOp |= enumOp.ConsoliderSolutions;
      }
      // Pas de pb rencontré, on va autoriser les autres opérations
      enumOp &= ~enumOp.Initialiser;
      return enumOp;
    }

    internal bool VerifieTailleEDEF(IFeedback parent, int nbSituationsInitiales)
    {
      // On stocke dans ED les duaux des situations de départ
      int tailleEnregistrementED = (Pilote.PlateauRaw.Count - TailleSituationsND) + nbSituationsInitiales;
      int tailleEnregistrementEF = TailleSituationsNF;
      FileInfo file = new FileInfo(Path.Combine(RacineData.FullName, "ED.dat"));
      long length = file.Length;
      bool bEDok = length % tailleEnregistrementED == 0 && length != 0;
      if (!bEDok)
      {
        parent?.Feedback(enumFeedbackHint.error, "Le fichier ED.dat n'a pas une taille correcte. Il est supprimé et va être réinitialisé.");
        file.Delete();
      }
      file = new FileInfo(Path.Combine(RacineData.FullName, "EF.dat"));
      length = file.Length;
      bool bEFok = length % tailleEnregistrementEF == 0 && length != 0;
      if (!bEFok)
      {
        parent?.Feedback(enumFeedbackHint.error, "Le fichier EF.dat n'a pas une taille correcte. Il est supprimé et va être réinitialisé.");
        file.Delete();
      }
      return bEDok & bEFok;
    }

    internal bool Initialiser(IFeedback parent, string filenameFicheDeJeu)
    {
      try
      {
        bool bInitPilote = false;
        // Existence de la fiche de jeu
        if (string.IsNullOrEmpty(filenameFicheDeJeu))
        {
          parent?.Feedback(enumFeedbackHint.error, "Veuillez spécifier le chemin et le nom de la fiche de jeu");
          return false;
        }
        if (!File.Exists(filenameFicheDeJeu))
        {
          parent?.Feedback(enumFeedbackHint.error, "Impossible de trouver la fiche de jeu spécifiée");
          return false;
        }
        FicheDeJeu = new FileInfo(filenameFicheDeJeu);
        // Controle du répertoire de données
        string filename = Path.GetFileNameWithoutExtension(FicheDeJeu.Name);
        string directoryName = Path.Combine(FicheDeJeu.DirectoryName, filename);
        RacineData = new DirectoryInfo(directoryName);
        if (!RacineData.Exists)
        {
          try
          {
            RacineData.Create();
          }
          catch (Exception ex)
          {
            parent?.Feedback(enumFeedbackHint.error, $"Impossible de créer le répertoire de données : {ex.Message}");
            return false;
          }
        }
        // Controle de l'existence du pilote
        FichierPilote = new FileInfo(Path.Combine(RacineData.FullName, filename + ".xml"));
        if (!FichierPilote.Exists)
        {
          bInitPilote = true;
        }
        // Chargement du contenu du pilote
        if (!bInitPilote)
        {
          try
          {
            Pilote = new Pilote(FichierPilote);
          }
          catch (Exception ex)
          {
            parent?.Feedback(enumFeedbackHint.error, $"Fichier pilote erroné : {ex.Message}, reconstruction d'un fichier pilote initial");
            bInitPilote = true;
          }
        }
        string descriptionPlateauInitial = File.ReadAllText(filenameFicheDeJeu);
        if (!bInitPilote)
        {
          // Controle de la cohérence du plateau initial dans la fiche de jeu et le pilote
          if (!Pilote.PlateauRaw.ControleDescription(descriptionPlateauInitial))
          {
            parent?.Feedback(enumFeedbackHint.error, @"Incohérence de description du plateau initial avec le plateau pilote,
            reconstruction d'un fichier pilote initial");
            bInitPilote = true;
          }
        }
        FileInfo FichierED = new FileInfo(Path.Combine(RacineData.FullName, "ED.dat"));
        FileInfo FichierEF = new FileInfo(Path.Combine(RacineData.FullName, "EF.dat"));
        if (bInitPilote)
        {
          Pilote = new Pilote();
          Pilote.Initialise(descriptionPlateauInitial);
          SauvePilote();
          if (FichierED.Exists)
          {
            FichierED.Delete();
          }
          if (FichierEF.Exists)
          {
            FichierEF.Delete();
          }
        }

      }
      catch (Exception exGenerale)
      {
        parent?.Feedback(enumFeedbackHint.error, $"Erreur générale : {exGenerale}");
        return false;
      }

      return true;
    }

    internal void SauvePilote()
    {
      Pilote.SauveConfig().Save(FichierPilote.FullName);
    }

    public FileStream OpenData(enumAccesData accesData)
    {
      string filename;
      FileMode fileMode;
      FileAccess fileAccess;
      switch (accesData)
      {
        case enumAccesData.EDdat:
          filename = Path.Combine(RacineData.FullName, "ED.dat");
          fileMode = FileMode.Open;
          fileAccess = FileAccess.Read;
          break;
        case enumAccesData.EDtmp:
          filename = Path.Combine(RacineData.FullName, "ED.tmp");
          fileMode = FileMode.Create;
          fileAccess = FileAccess.Write;
          break;
        case enumAccesData.EFdat:
          filename = Path.Combine(RacineData.FullName, "EF.dat");
          fileMode = FileMode.Open;
          fileAccess = FileAccess.Read;
          break;
        case enumAccesData.EFtmp:
        default:
          filename = Path.Combine(RacineData.FullName, "EF.tmp");
          fileMode = FileMode.Create;
          fileAccess = FileAccess.Write;
          break;
      }
      return new FileStream(filename, fileMode, fileAccess);
    }
    public void ReplaceData(enumAccesData accesData)
    {
      string filenameDat;
      string filenameTmp;
      string filenameOld;
      switch (accesData)
      {
        case enumAccesData.EDdat:
        case enumAccesData.EDtmp:
          filenameDat = Path.Combine(RacineData.FullName, "ED.dat");
          filenameTmp = Path.Combine(RacineData.FullName, "ED.tmp");
          filenameOld = Path.Combine(RacineData.FullName, "ED.old");
          if (File.Exists(filenameTmp))
          {
            if (File.Exists(filenameOld))
            {
              File.Delete(filenameOld);
            }
            if (File.Exists(filenameDat))
            {
              File.Move(filenameDat, filenameOld);
            }
            File.Move(filenameTmp, filenameDat);
          }
          break;
        case enumAccesData.EFdat:
        case enumAccesData.EFtmp:
        default:
          filenameDat = Path.Combine(RacineData.FullName, "EF.dat");
          filenameTmp = Path.Combine(RacineData.FullName, "EF.tmp");
          filenameOld = Path.Combine(RacineData.FullName, "EF.old");
          if (File.Exists(filenameTmp))
          {
            if (File.Exists(filenameOld))
            {
              File.Delete(filenameOld);
            }
            if (File.Exists(filenameDat))
            {
              File.Move(filenameDat, filenameOld);
            }
            File.Move(filenameTmp, filenameDat);
          }
          break;
      }
    }
    public bool Exist(enumAccesData accesData)
    {
      string filename;
      switch (accesData)
      {
        case enumAccesData.EDdat:
          filename = Path.Combine(RacineData.FullName, "ED.dat");
          break;
        case enumAccesData.EDtmp:
          filename = Path.Combine(RacineData.FullName, "ED.tmp");
          break;
        case enumAccesData.EFdat:
          filename = Path.Combine(RacineData.FullName, "EF.dat");
          break;
        case enumAccesData.EFtmp:
        default:
          filename = Path.Combine(RacineData.FullName, "EF.tmp");
          break;
      }
      return File.Exists(filename);
    }
    public long FileSize(enumAccesData accesData)
    {
      string filename;
      switch (accesData)
      {
        case enumAccesData.EDdat:
          filename = Path.Combine(RacineData.FullName, "ED.dat");
          break;
        case enumAccesData.EDtmp:
          filename = Path.Combine(RacineData.FullName, "ED.tmp");
          break;
        case enumAccesData.EFdat:
          filename = Path.Combine(RacineData.FullName, "EF.dat");
          break;
        case enumAccesData.EFtmp:
        default:
          filename = Path.Combine(RacineData.FullName, "EF.tmp");
          break;
      }
      return File.Exists(filename) ? (new FileInfo(filename)).Length : 0;
    }
    public void Supprime(enumAccesData accesData)
    {
      string filename;
      switch (accesData)
      {
        case enumAccesData.EDdat:
          filename = Path.Combine(RacineData.FullName, "ED.dat");
          break;
        case enumAccesData.EDtmp:
          filename = Path.Combine(RacineData.FullName, "ED.tmp");
          break;
        case enumAccesData.EFdat:
          filename = Path.Combine(RacineData.FullName, "EF.dat");
          break;
        case enumAccesData.EFtmp:
        default:
          filename = Path.Combine(RacineData.FullName, "EF.tmp");
          break;
      }
      if (File.Exists(filename))
      {
        File.Delete(filename);
      }
    }
  }
}
