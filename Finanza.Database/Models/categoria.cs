using System;
using System.Collections.Generic;

namespace Finanza.Database.Models;

public partial class categoria
{
    public int categoria_id { get; set; }

    public string categoria_nome { get; set; } = null!;

    public virtual ICollection<transazione> transazione { get; set; } = new List<transazione>();

    public virtual ICollection<trapasso> trapasso { get; set; } = new List<trapasso>();
}
