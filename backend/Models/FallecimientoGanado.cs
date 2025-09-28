using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class FallecimientoGanado
{
    public int IdfallecimientoGanado { get; set; }

    public int IdganadoFg { get; set; }

    public int IdusuarioFg { get; set; }

    public DateOnly FechaMg { get; set; }

    public string? CausaMg { get; set; }

    public virtual Ganado IdganadoFgNavigation { get; set; } = null!;

    public virtual Usuario IdusuarioFgNavigation { get; set; } = null!;
}
