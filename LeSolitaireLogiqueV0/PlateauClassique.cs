using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogiqueV0
{
  internal static class PlateauClassique
  {
    // Le plateau classique est constitué de 33 cases
    internal const int NbCases = 33;
    // Une position est décrite par 5 octets, seul le premier bit du cinquième octet est utilisé.
    internal const int TailleCle = 5;

    // permet d'accéder au bit de chaque case par son indice de 0 à NbCases-1
    // (évite de recalculer  i / 8)
    internal static readonly int[] Offset = {
      0, 0, 0, 0, 0, 0, 0, 0,
      1, 1, 1, 1, 1, 1, 1, 1,
      2, 2, 2, 2, 2, 2, 2, 2,
      3, 3, 3, 3, 3, 3, 3, 3,
      4,
    };
    
    // permet d'accéder au bit de chaque case par son indice de 0 à NbCases-1
    // (évite de recalculer  2^(i % 8) )
    internal static readonly byte[] Mask = {
      0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80,
      0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80,
      0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80,
      0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80,
      0x01,
    };
    internal static readonly byte[] MaskInverse = {
      0xFE, 0xFD, 0xFB, 0xF7, 0xEF, 0xDF, 0xBF, 0x7F,
      0xFE, 0xFD, 0xFB, 0xF7, 0xEF, 0xDF, 0xBF, 0x7F,
      0xFE, 0xFD, 0xFB, 0xF7, 0xEF, 0xDF, 0xBF, 0x7F,
      0xFE, 0xFD, 0xFB, 0xF7, 0xEF, 0xDF, 0xBF, 0x7F,
      0xFE,
    };
    
    // Fournit l'abscisse de la case représentée par son indice de 0 à 33-1
    internal static readonly int[] CoordX =
    {
            2, 3, 4,
            2, 3, 4,
      0, 1, 2, 3, 4, 5, 6,
      0, 1, 2, 3, 4, 5, 6,
      0, 1, 2, 3, 4, 5, 6,
            2, 3, 4,
            2, 3, 4,
    };
    // Fournit l'ordonnée de la case représentée par son indice de 0 à 33-1
    internal static readonly int[] CoordY =
    {
            0, 0, 0,
            1, 1, 1,
      2, 2, 2, 2, 2, 2, 2,
      3, 3, 3, 3, 3, 3, 3,
      4, 4, 4, 4, 4, 4, 4,
            5, 5, 5,
            6, 6, 6,
    };
    
    // Décrit les mouvements autorisés : si a et b sont occupés et c libre, le mouvement est possible : a et b sont retirés, c est comblé par une pierre
    internal const int NbMouvementsAutorises = 76;
    internal static readonly (int a, int b, int c)[] MouvementsAutorises = {
( 0, 1, 2),
( 0, 3, 8),
( 1, 4, 9),
( 2, 5,10),
( 2, 1, 0),
( 3, 4, 5),
( 3, 8,15),
( 4, 9,16),
( 5,10,17),
( 5, 4, 3),
( 6, 7, 8),
( 6,13,20),
( 7, 8, 9),
( 7,14,21),
( 8, 3, 0),
( 8, 9,10),
( 8,15,22),
( 8, 7, 6),
( 9, 4, 1),
( 9,10,11),
( 9,16,23),
( 9, 8, 7),
(10, 5, 2),
(10,11,12),
(10,17,24),
(10, 9, 8),
(11,18,25),
(11,10, 9),
(12,19,26),
(12,11,10),
(13,14,15),
(14,15,16),
(15, 8, 3),
(15,16,17),
(15,22,27),
(15,14,13),
(16, 9, 4),
(16,17,18),
(16,23,28),
(16,15,14),
(17,10, 5),
(17,18,19),
(17,24,29),
(17,16,15),
(18,17,16),
(19,18,17),
(20,13, 6),
(20,21,22),
(21,14, 7),
(21,22,23),
(22,15, 8),
(22,23,24),
(22,27,30),
(22,21,20),
(23,16, 9),
(23,24,25),
(23,28,31),
(23,22,21),
(24,17,10),
(24,25,26),
(24,29,32),
(24,23,22),
(25,18,11),
(25,24,23),
(26,19,12),
(26,25,24),
(27,22,15),
(27,28,29),
(28,23,16),
(29,24,17),
(29,28,27),
(30,27,22),
(30,31,32),
(31,28,23),
(32,29,24),
(32,31,30),
};

    // Liste des transformations 
    internal const int NbTransformations = 8;
    internal static readonly int[][] Transformations = {
new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,},
new int[] {12,19,26,11,18,25, 2, 5,10,17,24,29,32, 1, 4, 9,16,23,28,31, 0, 3, 8,15,22,27,30, 7,14,21, 6,13,20,},
new int[] {32,31,30,29,28,27,26,25,24,23,22,21,20,19,18,17,16,15,14,13,12,11,10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0,},
new int[] {20,13, 6,21,14, 7,30,27,22,15, 8, 3, 0,31,28,23,16, 9, 4, 1,32,29,24,17,10, 5, 2,25,18,11,26,19,12,},
new int[] { 2, 1, 0, 5, 4, 3,12,11,10, 9, 8, 7, 6,19,18,17,16,15,14,13,26,25,24,23,22,21,20,29,28,27,32,31,30,},
new int[] {30,31,32,27,28,29,20,21,22,23,24,25,26,13,14,15,16,17,18,19, 6, 7, 8, 9,10,11,12, 3, 4, 5, 0, 1, 2,},
new int[] { 6,13,20, 7,14,21, 0, 3, 8,15,22,27,30, 1, 4, 9,16,23,28,31, 2, 5,10,17,24,29,32,11,18,25,12,19,26,},
new int[] {26,19,12,25,18,11,32,29,24,17,10, 5, 2,31,28,23,16, 9, 4, 1,30,27,22,15, 8, 3, 0,21,14, 7,20,13, 6,},
    };
    
    // Compare une position donnée avec une des positions enregistrées dans un noeud du BTree
    // ATTENTION : indiceInNoeud est exprimé en nombre d'octets de décalage % début du noeud
    // Permet aussi de comparer deux positions données.
    internal static int ComparePositions(byte[] position, byte[] positionsNoeud, int indiceInNoeud)
    {
      for (int i = 0; i < TailleCle; i++)
      {
        byte a = position[i], b = positionsNoeud[indiceInNoeud + i];
        if (a < b) return -1;
        if (a > b) return 1;
      }
      return 0;
    }

  }
}
