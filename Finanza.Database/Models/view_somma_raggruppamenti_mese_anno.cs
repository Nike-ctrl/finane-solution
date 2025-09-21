using System;
using System.Collections.Generic;

namespace Finanza.Database.Models;

public partial class view_somma_raggruppamenti_mese_anno
{
    public int? raggruppamento_conto_id { get; set; }

    public int? anno { get; set; }

    public int? mese { get; set; }

    public DateTime? data { get; set; }

    public decimal? sum_valore_reale { get; set; }
}
