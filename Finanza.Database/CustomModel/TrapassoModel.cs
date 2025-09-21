using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanza.Database.CustomModel
{

    /// <summary>
    /// Classe di riferimento per il trapasso
    /// </summary>
    public class TrapassoModel
    {
        public int ScrContoId { get; set; }
        public int DestContoId { get; set; }
        public decimal Valore { get; set; }
        public DateTime Dt { get; set; }
        public int? CategoriaId { get; set; }
        public string? Descrizione { get; set; }
    }
}
