using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class Pesaje
{
    public int Idpesaje { get; set; }

    public int IdganadoPj { get; set; }

    public int IdusuarioPj { get; set; }

    public DateOnly? FechaPj { get; set; }

    public int? PesoPj { get; set; }

    public int? CondicionCorporal { get; set; }

    public string? Observaciones { get; set; }

    public virtual Ganado IdganadoPjNavigation { get; set; } = null!;

    public virtual Usuario IdusuarioPjNavigation { get; set; } = null!;
}
