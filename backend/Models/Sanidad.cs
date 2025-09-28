using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class Sanidad
{
    public int Idsanidad { get; set; }

    public int IdganadoSn { get; set; }

    public int IdusuarioSn { get; set; }

    public DateOnly FechaSn { get; set; }

    public string? DescripcionSn { get; set; }

    public int? ProductoSn { get; set; }

    public string? ObservacionesSn { get; set; }

    public virtual Ganado IdganadoSnNavigation { get; set; } = null!;

    public virtual Usuario IdusuarioSnNavigation { get; set; } = null!;

    public virtual ProductoSanidad? ProductoSnNavigation { get; set; }
}
