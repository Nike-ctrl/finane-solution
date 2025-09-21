using System;
using System.Collections.Generic;

namespace Finanza.Database.Models;

public partial class view_valore_cumulativo_mensile
{
    public int? raggruppamento_conto_id { get; set; }

    public int? anno { get; set; }

    public int? mese { get; set; }

    public DateTime? data { get; set; }

    public decimal? valore_mensile { get; set; }

    public decimal? valore_cumulativo { get; set; }
}
