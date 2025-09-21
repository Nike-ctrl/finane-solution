using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanza.Database.CustomModel
{

    /// <summary>
    /// Classe per il sommario delle categorie
    /// </summary>
    public class SommarioCategoria
    {
        public string categoria_nome { get; set; }

        public decimal sum_valore { get; set; }

        public decimal sum_valore_reale { get; set; }

    }
}
