using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanza.Database.CustomModel
{
    public class ValoreCumulativoMensileConDifferenza
    {

        public int anno { get; set; }

        public int mese { get; set; }

        public DateTime data { get; set; }

        public decimal valore_cumulativo { get; set; }

        public decimal differenza_col_primo { get; set; }

        public decimal valore_mensile { get; set; }

        
    }
}
