using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTree
{
  internal class NiveauPersistant
  {
    protected readonly bool IsFeuille;
    // 1+nb max d'éléments, nb max liens vers noeuds enfants
    protected readonly int Ordre;
    // nb octets décrivant un élément stocké
    protected readonly int TailleElement;
    // taille en octets du buffer contenant les descriptions d'éléments,
    // y compris l'octet initial qui donne le nombre actuel d'éléments stockés.
    protected readonly int TailleData;

    private FileStream FileStreamFichier;
    private BinaryWriter BinaryWriterFichier;
    private BinaryReader BinaryReaderFichier;
    // taille cumulée des deux buffers, celui des éléments, et celui des enfants si on n'est pas dans une feuille
    private readonly int TailleNoeud;
    // taille en octets du buffer des indices de type UInt32 des enfants
    // 0 si feuille
    private readonly int TailleOffset;
    public NiveauPersistant(FileInfo fileInfo, int ordre, int tailleSituation, bool isFeuille)
    {
      IsFeuille = isFeuille;
      Ordre = ordre;
      TailleElement = tailleSituation;
      TailleData = 1 + tailleSituation * (ordre - 1);

      FileAccess access = FileAccess.ReadWrite;
      FileStreamFichier = new FileStream(fileInfo.FullName, FileMode.Open, access, FileShare.None);
      long sz = FileStreamFichier.Length;
      if (!IsFeuille)
      {
        TailleOffset = sizeof(UInt32) * ordre;
        BinaryWriterFichier = new BinaryWriter(FileStreamFichier);
        BinaryReaderFichier = new BinaryReader(FileStreamFichier);
      }
      TailleNoeud = TailleData + TailleOffset;
      if (sz == 0 || (sz % (TailleNoeud)) != 0)
      {
        FileStreamFichier.Close();
        throw new ApplicationException($"Le fichier {fileInfo.FullName} a une taille incorrecte");
      }
    }

    internal NoeudPersistant ChargeNoeud(UInt32 offsetNoeud)
    {
      NoeudPersistant stockageNoeud = new NoeudPersistant(Ordre, TailleElement, IsFeuille);
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

    internal void AlloueNoeud(DataRemonteeVolatile data)
    {
      data.enfantPlus = new NoeudPersistant(Ordre, TailleElement, IsFeuille);
      // On alloue de la place disque en allongeant la taille du fichier
      long l = FileStreamFichier.Length;
      ((DataRemonteePersistant)data).offsetEnfantPlus = (UInt32)(l / TailleNoeud);
      l += TailleNoeud;
      FileStreamFichier.SetLength(l);
      data.enfantPlus.dirty = true;
    }

    internal void Flush(NoeudPersistant noeud, uint offsetNoeud)
    {
      long idxData = offsetNoeud * TailleNoeud;
      FileStreamFichier.Seek(idxData, SeekOrigin.Begin);
      FileStreamFichier.Write(noeud.data, 0, TailleData);
      if (!noeud.IsFeuille)
      {
        for (int idxOffset = 0; idxOffset < Ordre; idxOffset++)
        {
          BinaryWriterFichier.Write(((NoeudPersistant)noeud).OffsetEnfants[idxOffset]);
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
