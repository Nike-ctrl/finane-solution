using System;
using System.Collections.Generic;

namespace Finanza.Database.Models;

public partial class view_somma_conto_anno_mese
{
    public string? conto_nome { get; set; }

    public int? anno { get; set; }

    public int? mese { get; set; }

    public decimal? sum_mese_valore_reale { get; set; }
}
