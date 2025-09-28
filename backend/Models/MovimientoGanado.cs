using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class MovimientoGanado
{
    public int IdmovimientoGanado { get; set; }

    public int IdganadoMg { get; set; }

    public int IdusuarioMg { get; set; }

    public int IdtipoMovimientoMg { get; set; }

    public int? IdfincaOrigne { get; set; }

    public int? IdfincaDestino { get; set; }

    public string? ValorMg { get; set; }

    public string? ObservacionesMg { get; set; }

    public virtual Ganado IdganadoMgNavigation { get; set; } = null!;

    public virtual TipoMovimiento IdtipoMovimientoMgNavigation { get; set; } = null!;

    public virtual Usuario IdusuarioMgNavigation { get; set; } = null!;
}
