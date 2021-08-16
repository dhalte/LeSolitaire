using BTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeSolitaireLogique.Services
{
  internal class EtapeRechercheEnProfondeur
  {
    internal int Mvt;
    internal BTreeVolatile Stock;
    internal Plateau.EnumerationNouvellesSituationsNormalisees Enumeration;
  }
}
