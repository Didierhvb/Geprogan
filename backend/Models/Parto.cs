using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class Parto
{
    public int Idparto { get; set; }

    public int IdganadoPt { get; set; }

    public int IdusuarioPt { get; set; }

    public int? IdcriaPt { get; set; }

    public DateOnly FechaPt { get; set; }

    public string? ObservacionesPt { get; set; }

    public virtual Ganado? IdcriaPtNavigation { get; set; }

    public virtual Ganado IdganadoPtNavigation { get; set; } = null!;

    public virtual Usuario IdusuarioPtNavigation { get; set; } = null!;

    public virtual ICollection<SeguimientoParto> SeguimientoPartos { get; set; } = new List<SeguimientoParto>();
}
