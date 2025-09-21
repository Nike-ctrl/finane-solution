using System;
using System.Collections.Generic;

namespace Finanza.Database.Models;

public partial class raggruppamento_conto
{
    public int raggruppamento_conto_id { get; set; }

    public string raggruppamento_conto_nome { get; set; } = null!;

    public virtual ICollection<vincolo_gruppo> vincolo_gruppo { get; set; } = new List<vincolo_gruppo>();
}
