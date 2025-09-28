using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class TipoMovimiento
{
    public int IdtipoMovimiento { get; set; }

    public string NombreMovimientoTm { get; set; } = null!;

    public virtual ICollection<MovimientoGanado> MovimientoGanados { get; set; } = new List<MovimientoGanado>();
}
