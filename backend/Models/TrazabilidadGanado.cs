using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class TrazabilidadGanado
{
    public int IdtrazabilidadGanado { get; set; }

    public int IdganadoTg { get; set; }

    public int IdusuarioTg { get; set; }

    public int IdFincaTg { get; set; }

    public int IdTipoGanadoTg { get; set; }

    public string MarcaGanadoTg { get; set; } = null!;

    public string RazaTg { get; set; } = null!;

    public string CaracteristicasTg { get; set; } = null!;

    public string NombreGanadoTg { get; set; } = null!;

    public string SexoTg { get; set; } = null!;

    public string NumeroIdTg { get; set; } = null!;

    public string NumeroInventarioTg { get; set; } = null!;

    public DateOnly FechaCambioTg { get; set; }

    public string? ComentarioTg { get; set; }

    public string UrlImagenTg { get; set; } = null!;

    public virtual Ganado IdganadoTgNavigation { get; set; } = null!;

    public virtual Usuario IdusuarioTgNavigation { get; set; } = null!;
}
