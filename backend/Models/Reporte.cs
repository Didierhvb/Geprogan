using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class Reporte
{
    public int Idreporte { get; set; }

    public int IdusuarioRp { get; set; }

    public DateOnly FechaRp { get; set; }

    public string Descripcion { get; set; } = null!;

    public virtual Usuario IdusuarioRpNavigation { get; set; } = null!;
}
