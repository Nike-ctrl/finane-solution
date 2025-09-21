using System;
using System.Collections.Generic;

namespace Finanza.Database.Models;

public partial class view_sommario_transazioni
{
    public int? transazione_id { get; set; }

    public int? conto_id { get; set; }

    public int? tipo_transazione_id { get; set; }

    public int? categoria_id { get; set; }

    public int? trapasso_id { get; set; }

    public DateTime? data { get; set; }

    public decimal? valore { get; set; }

    public decimal? valore_reale { get; set; }

    public string? conto_nome { get; set; }

    public string? categoria_nome { get; set; }

    public string? tipo_transazione_nome { get; set; }

    public string? descrizione { get; set; }

    public int? anno { get; set; }

    public int? mese { get; set; }
}
