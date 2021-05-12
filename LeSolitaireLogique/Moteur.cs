using LeSolitaireStockage;
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
    // Le répertoire contient ou va contenir toutes les informations persistentes liées à la recherche
    private DirectoryInfo Repertoire;
    // L'objet à qui seront communiquées diverses opérations dans les traitements en tâche de fond
    private Feedback Parent;
    // Toutes les informations et opérations relatives au plateau du jeu
    private Services.Plateau Plateau;
    private const int Ordre = 32;
    // On prévoit un emplacement dans la liste pour chaque étape.
    // Lors de la recherche en largeur, seules la dernière étape et la nouvelle sont utilisés
    // Lors de la recherche en profondeur, seule la dernière est utilisée
    // Mais lors de la consolidation d'une solution, on aura besoin d'accéder aux autres étapes.
    private List<Stockage> Stockages;
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
      Stockages = new List<Stockage>();
      DirectoryInfo repertoire = new DirectoryInfo(Path.Combine(Repertoire.FullName, "0"));
      Stockage stockage = new Stockage(repertoire, Services.Plateau.TailleDescriptionSituationEtMasque, Ordre, Plateau);
      foreach (byte[] situationInitiale in situationsInitiales)
      {
        fixed (byte* pSituationInitiale = situationInitiale)
        {
          stockage.InsertOrUpdate(pSituationInitiale);
        }
      }
    }
  }
}
