using System;
using System.Collections.Generic;

namespace Finanza.Database.Models;

public partial class transazione
{
    public int transazione_id { get; set; }

    public int conto_id { get; set; }

    public int tipo_transazione_id { get; set; }

    public int? trapasso_id { get; set; }

    public DateTime data { get; set; }

    /// <summary>
    /// indica il valore positivo di inserimento
    /// </summary>
    public decimal valore { get; set; }

    /// <summary>
    /// questo campo viene modifica dal trigger before insert, si occupa di capire se deve essere positivo o negati dal tipo di transazione
    /// </summary>
    public decimal valore_reale { get; set; }

    public string? descrizione { get; set; }

    public int? categoria_id { get; set; }

    public int anno { get; set; }

    public int mese { get; set; }

    public DateTime creazione { get; set; }

    public virtual categoria? categoria { get; set; }

    public virtual conto conto { get; set; } = null!;

    public virtual tipo_transazione tipo_transazione { get; set; } = null!;

    public virtual trapasso? trapasso { get; set; }

    public virtual ICollection<trapasso> trapassotransazione_destinazione { get; set; } = new List<trapasso>();

    public virtual ICollection<trapasso> trapassotransazione_sorgente { get; set; } = new List<trapasso>();
}
