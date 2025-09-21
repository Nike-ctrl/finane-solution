using System;
using System.Collections.Generic;

namespace Finanza.Database.Models;

public partial class conto
{
    public int conto_id { get; set; }

    public string conto_nome { get; set; } = null!;

    public virtual ICollection<transazione> transazione { get; set; } = new List<transazione>();

    public virtual ICollection<vincolo_gruppo> vincolo_gruppo { get; set; } = new List<vincolo_gruppo>();
}
