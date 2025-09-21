using System;
using System.Collections.Generic;

namespace Finanza.Database.Models;

public partial class view_somma_categoria_anno_mese_conto
{
    public string? categoria_nome { get; set; }

    public int? conto_id { get; set; }

    public int? mese { get; set; }

    public int? anno { get; set; }

    public decimal? sum_valore { get; set; }

    public decimal? sum_valore_reale { get; set; }
}
