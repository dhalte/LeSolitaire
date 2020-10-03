using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  public class Solution
  {
    public SituationRaw SituationInitialeRaw;
    public Situation SituationInitiale;
    // Etape 1 : établir ED.dat, qui liste des situations de départ, 
    //           chacune associée aux indices des situations initiales qui y mènent
    // Etape 2 : établir EF.dat, qi liste des situations avancées, proches des situations gagnantes
    // Etape 3 : trouver des chemins entre des éléments SD de ED et des éléments SF de EF
    //           enregistrer ces chemins dans des pré-solutions
    // Etape 4 : reprendre les pré-solutions et rechercher les situations initiales SI
    //           qui y mènent et les mouvements nécessaires.
    //           On ne s'occupe que des SI pour lesquels on n'a pas encore fait l'opération
    //           On enregistre ces SI et mouvements dans une Solution où Complete est à false
    // Etape 5 : reprendre ces solutions avec Complete à false pour trouver des mouvements qui 
    //           résolvent la SF associée. Alors la solution peut passer à Complete=true
    public bool Complete;
    public List<SolutionMouvement> Mouvements = new List<SolutionMouvement>();
  }
}
