using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique
{
  class LogiqueConfiguration
  {
    // Le fichier fourni, ne contient que le plateau initial
    public FileInfo FichierInitialisation;
    // Le répertoire qui porte le nom du fichier FichierInitialisation et dans lequel on stocke les données de la recherche
    public DirectoryInfo RacineData;
    // Le contenu du fichier FichierInitialisation
    public Config ConfigInitialisation;
    // Le fichier pilote
    public FileInfo FichierPilote;
    // Le contenu du fichier FichierPilote
    public Config ConfigPilote;
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
  }
}
