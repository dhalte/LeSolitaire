using BTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  // Cette classe permet de lancer tous les traitements liés aux opérations de recherche de solution
  public class Moteur
  {
    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    // Le répertoire contient ou va contenir toutes les informations persistentes liées à la recherche
    private readonly DirectoryInfo Repertoire;
    // L'objet à qui seront communiquées diverses opérations dans les traitements en tâche de fond
    private readonly Feedback Parent;
    // Toutes les informations et opérations relatives au plateau du jeu
    private Services.Plateau Plateau;
    private const int Ordre = 32;
    // On prévoit un emplacement dans la liste pour chaque étape.
    // Lors de la recherche en largeur, seules la dernière étape et la nouvelle sont utilisés
    // Lors de la recherche en profondeur, seule la dernière est utilisée
    // Mais lors de la consolidation d'une solution, on aura besoin d'accéder aux autres étapes.
    private List<BTreePersistant> Stockages;
    public Moteur(string repertoire, Feedback parent)
    {
      Repertoire = new DirectoryInfo(repertoire);
      Parent = parent;
    }
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
        RechercheEnLargeurInitStockage();
        if (Plateau.Reprise == Services.EnumReprise.None)
        {
          Parent.Feedback(FeedbackHint.info, $"Création du niveau {Stockages.Count - 1}");
        }
        else
        {
          Parent.Feedback(FeedbackHint.info, $"Reprise du niveau {Stockages.Count - 1}");
        }
        Parent.Feedback(FeedbackHint.info, $"niveau;nb insérés;durée(ms)");
        BTreePersistant origine = Stockages[Stockages.Count - 2], destination = Stockages[Stockages.Count - 1];
        if (Plateau.Reprise == Services.EnumReprise.None) Plateau.SituationReprise = null;
        foreach (byte[] situation in origine.EnumereElements(Plateau.SituationReprise))
        {
          nbSituationsTraitees++;
          if (logger.IsTraceEnabled)
          {
            logger.Trace($"{Environment.NewLine}{new string('-', 30)}{Environment.NewLine}Traitement de{Environment.NewLine}{Plateau.Dump(situation)}Flag:{situation[Services.Plateau.TailleDescriptionSituationEtMasque - 1].ToString("X").PadLeft(2, '0')}");
          }
          Services.Plateau.EnumerationNouvellesSituationsNormalisees x = new Services.Plateau.EnumerationNouvellesSituationsNormalisees(Plateau, situation);
          while (x.Next())
          {
            InsertOrUpdateResult result = destination.InsertOrUpdate(x.NouvelleSituation);
            if (logger.IsTraceEnabled)
            {
              logger.Trace($"Résultat{Environment.NewLine}{Plateau.Dump(x.NouvelleSituation)}{result} Flag:{x.NouvelleSituation[Services.Plateau.TailleDescriptionSituationEtMasque - 1].ToString("X").PadLeft(2, '0')}");
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
        }
        destination.Flush();
        foreach (BTreePersistant stockage in Stockages)
        {
          stockage.Close();
        }
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
      }
    }

    private void RechercheEnLargeurInitStockage()
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
      if (Plateau.Reprise == Services.EnumReprise.None)
      {
        DirectoryInfo rep = new DirectoryInfo(Path.Combine(Repertoire.FullName, $"{Stockages.Count}"));
        Stockages.Add(new BTreePersistant(rep, Services.Plateau.TailleDescriptionSituationEtMasque, Ordre, Plateau));
        Stockages[Stockages.Count - 1].InitBTree();
      }
    }

    public void Charger()
    {
      Plateau = Services.Plateau.ChargerFichierPilote(Repertoire);
      Parent.Feedback(FeedbackHint.trace, $"Plateau chargé :{Environment.NewLine}{Plateau.DescriptionPlateau()}");
    }
  }
}
