using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class FincaProduccion
{
    public int IdfincaProduccion { get; set; }

    public int IdfincaFp { get; set; }

    public decimal TotalProducido { get; set; }

    public virtual Finca IdfincaFpNavigation { get; set; } = null!;
}
