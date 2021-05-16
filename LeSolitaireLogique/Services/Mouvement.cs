namespace LeSolitaireLogique.Services
{
    // Liste tous les mouvements possibles,
    // Chaque triplet (c1, c2, c3) représente les indices de 3 cases contigües alignées du plateau
    internal class Mouvement
    {
      internal int c1;
      internal int c2;
      internal int c3;

      public Mouvement(int v1, int v2, int v3)
      {
        c1 = v1;
        c2 = v2;
        c3 = v3;
      }
    }
}
