using System;
using System.Collections.Generic;

namespace Finanza.Database.Models;

public partial class tipo_transazione
{
    public int tipo_transazione_id { get; set; }

    public string tipo_transazione_nome { get; set; } = null!;

    public bool direzione { get; set; }

    public virtual ICollection<transazione> transazione { get; set; } = new List<transazione>();
}
