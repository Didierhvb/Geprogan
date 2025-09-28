using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class SeguimientoParto
{
    public int IdseguimientoParto { get; set; }

    public int IdpartoSp { get; set; }

    public int IdusuarioSp { get; set; }

    public int DiasLactancia { get; set; }

    public decimal ProduccionTotalSp { get; set; }

    public DateOnly? FechaDestete { get; set; }

    public virtual Parto IdpartoSpNavigation { get; set; } = null!;

    public virtual Usuario IdusuarioSpNavigation { get; set; } = null!;
}
