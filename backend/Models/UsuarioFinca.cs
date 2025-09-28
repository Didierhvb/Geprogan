using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class UsuarioFinca
{
    public int IdusuarioFinca { get; set; }

    public int IdusuarioUf { get; set; }

    public int IdfincaUf { get; set; }

    public virtual Finca IdfincaUfNavigation { get; set; } = null!;

    public virtual Usuario IdusuarioUfNavigation { get; set; } = null!;
}
