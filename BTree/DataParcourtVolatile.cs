namespace BTree
{
  internal class DataParcourtVolatile
  {
    internal NoeudVolatile noeud;
    internal int idxElt;

    public DataParcourtVolatile(NoeudVolatile noeud = null, int idxElt = 0)
    {
      this.noeud = noeud;
      this.idxElt = idxElt;
    }
  }

}
