namespace BTree
{
  internal class DataParcourtPersistant:DataParcourtVolatile
  {

    public DataParcourtPersistant(NoeudPersistant noeud = null, int idxElt = 0)
    {
      this.noeud = noeud;
      this.idxElt = idxElt;
    }
  }

}
