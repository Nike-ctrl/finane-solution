using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanza.Database.CustomModel
{
    public class SommarioCategoriaPerConto
    {
        public int conto_id { get; set; }


        public string conto_nome { get; set; }

        public int tipo_transazione_id { get; set; }

        public string tipo_transazione_nome { get; set; }

        public int categoria_id { get; set; }

        public string categoria_nome { get; set; }

        public decimal sum_valore_reale_abs { get; set; }
    }
}
