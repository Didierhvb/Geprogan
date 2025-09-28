using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class Produccion
{
    public int Idproduccion { get; set; }

    public int IdganadoPr { get; set; }

    public int IdusuarioPr { get; set; }

    public DateOnly FechaPr { get; set; }

    public string HorarioPr { get; set; } = null!;

    public decimal CantidadPr { get; set; }

    public string? ObservacionesPr { get; set; }

    public virtual Ganado IdganadoPrNavigation { get; set; } = null!;

    public virtual Usuario IdusuarioPrNavigation { get; set; } = null!;
}
