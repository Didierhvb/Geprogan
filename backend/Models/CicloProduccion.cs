using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class CicloProduccion
{
    public int Idciclo { get; set; }

    public int IdganadoCp { get; set; }

    public int IdusuarioCp { get; set; }

    public DateOnly FechaInicioCp { get; set; }

    public DateOnly FechaFinCp { get; set; }

    public decimal CantidadTotalCp { get; set; }

    public string? ObservacionesCp { get; set; }

    public virtual Ganado IdganadoCpNavigation { get; set; } = null!;

    public virtual Usuario IdusuarioCpNavigation { get; set; } = null!;
}
