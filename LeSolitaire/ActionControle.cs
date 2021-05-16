using LeSolitaireLogique;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaire
{
  class ActionControle : Feedback
  {
    // Utilisée pour enregistrer les listes saisies dans les boites de dialogue
    public const string KeyListe = "LeSolitaire";
    private bool IsLoaded = false;
    private bool IsRunning = false;
    private Moteur Moteur;
    private readonly Feedback Parent;
    internal ActionControle(Feedback parent)
    {
      Parent = parent;
    }
    internal bool AutoriserInitialiser()
    {
      return !IsRunning;
    }

    internal bool AutoriserCharger()
    {
      return !IsRunning;
    }

    internal bool AutoriserRechercheEnLargeur()
    {
      return IsLoaded && !IsRunning;
    }

    internal bool AutoriserRechercheEnProfondeur()
    {
      return IsLoaded && !IsRunning;
    }

    internal bool AutoriserSuspendre()
    {
      return IsRunning;
    }

    internal bool AutoriserVueResultats()
    {
      return IsLoaded;
    }

    internal void Initialiser(string pathFichierDescriptionPlateau, string pathRepertoireStockage)
    {
      IsLoaded = false;
      string description = File.ReadAllText(pathFichierDescriptionPlateau);
      Moteur = new Moteur(pathRepertoireStockage, this);
      Moteur.Initialise(description);
      IsLoaded = true;
    }
    internal void Charger(string pathRepertoireStockage)
    {
      IsLoaded = false;
      Moteur = new Moteur(pathRepertoireStockage, this);
      Moteur.Charger();
      IsLoaded = true;
    }

    public void Feedback(FeedbackHint hint, string msg)
    {
      switch (hint)
      {
        case FeedbackHint.trace:
        case FeedbackHint.info:
        case FeedbackHint.warning:
          break;
        case FeedbackHint.startOfJob:
          IsRunning = true;
          break;
        case FeedbackHint.error:
        case FeedbackHint.endOfJob:
        default:
          IsRunning = false;
          break;
      }
      Parent.Feedback(hint, msg);
    }

    internal void LanceRechercheEnLargeur()
    {
      Moteur.LanceRechercheEnLargeur();
    }
  }
}
