using System;
using System.Collections.Generic;

namespace Finanza.Database.Models;

public partial class vincolo_gruppo
{
    public int vincolo_gruppo_id { get; set; }

    public int raggruppamento_conto_id { get; set; }

    public int conto_id { get; set; }

    public virtual conto conto { get; set; } = null!;

    public virtual raggruppamento_conto raggruppamento_conto { get; set; } = null!;
}
