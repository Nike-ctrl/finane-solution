using System;
using System.Collections.Generic;

namespace Finanza.Database.Models;

/// <summary>
/// tabella dove vengono salvate le note
/// </summary>
public partial class nota
{
    /// <summary>
    /// pk
    /// </summary>
    public int nota_id { get; set; }

    /// <summary>
    /// indica la data di creazione della nota
    /// </summary>
    public DateTime creation { get; set; }

    public string testo_nota { get; set; } = null!;

    /// <summary>
    /// anno della nota
    /// </summary>
    public int anno { get; set; }

    /// <summary>
    /// mese della nota
    /// </summary>
    public int mese { get; set; }
}
