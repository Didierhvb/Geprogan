using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class SeguimientoReproduccion
{
    public int IdseguimientoReproduccion { get; set; }

    public int IdganadoSr { get; set; }

    public int IdusuarioSr { get; set; }

    public DateOnly FechaSr { get; set; }

    public string? Servicio { get; set; }

    public string? Palpacion { get; set; }

    public string? ObservacionesSr { get; set; }

    public virtual Ganado IdganadoSrNavigation { get; set; } = null!;

    public virtual Usuario IdusuarioSrNavigation { get; set; } = null!;
}
