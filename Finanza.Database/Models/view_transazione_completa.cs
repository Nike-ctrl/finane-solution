using System;
using System.Collections.Generic;

namespace Finanza.Database.Models;

public partial class view_transazione_completa
{
    public int? transazione_id { get; set; }

    public DateTime? data { get; set; }

    public DateTime? creazione { get; set; }

    public decimal? valore_reale { get; set; }

    public string? tipo_transazione_nome { get; set; }

    public string? conto_nome { get; set; }

    public string? categoria_nome { get; set; }

    public string? descrizione { get; set; }
}
