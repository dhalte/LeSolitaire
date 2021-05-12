using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireStockage
{
  internal class StockageNiveau
  {
    private FileStream FileStreamFichier;
    private BinaryWriter BinaryWriterFichier;
    private BinaryReader BinaryReaderFichier;
    // 0 == fichier des feuilles de l'arbre
    private readonly int IdxFichier;
    private bool IsFeuille => IdxFichier == 0;
    // 1+nb max d'éléments, nb max liens vers noeuds enfants
    private readonly int Ordre;
    // nb octets décrivant un élément stocké
    private readonly int TailleElement;
    // taille en octets du buffer contenant les descriptions d'éléments,
    // y compris l'octet initial qui donne le nombre actuel d'éléments stockés.
    private readonly int TailleData;
    // taille en octets du buffer des indices de type UInt32 des enfants
    // 0 si feuille
    private readonly int TailleOffset;
    // taille cumulée des deux buffers, celui des éléments, et celui des enfants si on n'est pas dans une feuille
    private readonly int TailleNoeud;
    public StockageNiveau(int idxFichier, FileInfo fileInfo, int ordre, int tailleSituation, bool readOnly)
    {
      IdxFichier = idxFichier;
      Ordre = ordre;
      TailleElement = tailleSituation;
      FileAccess access = readOnly ? FileAccess.Read : FileAccess.ReadWrite;
      FileStreamFichier = new FileStream(fileInfo.FullName, FileMode.Open, access, FileShare.None);
      long sz = FileStreamFichier.Length;
      TailleData = 1 + tailleSituation * (ordre - 1);
      if (!IsFeuille)
      {
        TailleOffset = sizeof(UInt32) * ordre;
        if (!readOnly)
        {
          BinaryWriterFichier = new BinaryWriter(FileStreamFichier);
        }
        BinaryReaderFichier = new BinaryReader(FileStreamFichier);
      }
      TailleNoeud = TailleData + TailleOffset;
      if (sz == 0 || (sz % (TailleNoeud)) != 0)
      {
        FileStreamFichier.Close();
        throw new ApplicationException($"Le fichier {fileInfo.FullName} a une taille incorrecte");
      }
    }

    internal StockageNoeud ChargeNoeud(UInt32 offsetNoeud)
    {
      StockageNoeud stockageNoeud = new StockageNoeud(Ordre, TailleElement, IdxFichier == 0);
      long offsetBuffer = offsetNoeud * TailleNoeud;
      FileStreamFichier.Seek(offsetBuffer, SeekOrigin.Begin);
      if (FileStreamFichier.Read(stockageNoeud.data, 0, TailleData) != TailleData)
      {
        throw new ApplicationException($"Impossible de lire le noeud à l'offset {offsetNoeud}");
      }
      if (!IsFeuille)
      {
        // Le pointeur de lecture est déplacé lors d'un Read, aussi il pointe actuellement à la bonne place
        for (int idxOffset = 0; idxOffset < Ordre; idxOffset++)
        {
          // Déclenche une exception si on dépasse la taille du fichier
          stockageNoeud.OffsetEnfants[idxOffset] = BinaryReaderFichier.ReadUInt32();
        }
      }
      return stockageNoeud;
    }
    internal StockageNoeud AlloueNoeud(out uint offsetNouveauNoeud)
    {
      StockageNoeud stockageNoeud = new StockageNoeud(Ordre, TailleElement, IsFeuille);
      // On alloue de la place disque en allongeant la taille du fichier
      long l = FileStreamFichier.Length;
      offsetNouveauNoeud = (UInt32)(l / TailleNoeud);
      l += TailleNoeud;
      FileStreamFichier.SetLength(l);
      stockageNoeud.dirty = true;
      return stockageNoeud;
    }

    internal void Flush(StockageNoeud noeud, uint offsetNoeud)
    {
      long idxData = offsetNoeud * TailleNoeud;
      FileStreamFichier.Seek(idxData, SeekOrigin.Begin);
      FileStreamFichier.Write(noeud.data, 0, TailleData);
      if (!noeud.IsFeuille)
      {
        for (int idxOffset = 0; idxOffset < Ordre; idxOffset++)
        {
          BinaryWriterFichier.Write(noeud.OffsetEnfants[idxOffset]);
        }
      }
      noeud.dirty = false;
    }

    internal void Close()
    {
      FileStreamFichier.Close();
      FileStreamFichier = null;
      BinaryReaderFichier = null;
      BinaryWriterFichier = null;
    }
  }
}
