using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTree
{
  // Résultat d'une opération InsertOrUpdate
  public enum InsertOrUpdateResult
  {
    // L'élément existe déjà et la fonction de rappel a indiqué qu'elle n'avait pas mis à jour la partie variable
    NoChange = 0,
    // L'élément n'existait pas, il a été inséré
    Inserted = 1,
    // L'élément existe déjà et la fonction de rappel a indiqué qu'elle avait mis à jour la partie variable
    Updated = 2
  }
}
