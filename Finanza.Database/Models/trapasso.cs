using System;
using System.Collections.Generic;

namespace Finanza.Database.Models;

public partial class trapasso
{
    public int trapasso_id { get; set; }

    /// <summary>
    /// id transazione sorgente
    /// </summary>
    public int transazione_sorgente_id { get; set; }

    /// <summary>
    /// id transazione di destinazione
    /// </summary>
    public int transazione_destinazione_id { get; set; }

    public DateTime data_transazione { get; set; }

    public decimal valore_transazione { get; set; }

    public string? descizione { get; set; }

    public int? categoria_id { get; set; }

    public virtual categoria? categoria { get; set; }

    public virtual ICollection<transazione> transazione { get; set; } = new List<transazione>();

    public virtual transazione transazione_destinazione { get; set; } = null!;

    public virtual transazione transazione_sorgente { get; set; } = null!;
}
