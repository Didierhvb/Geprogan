using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class FamiliaGanado
{
    public int IdfamiliaGanado { get; set; }

    public int IdtipoParentescoFa { get; set; }

    public int IdganadoFa { get; set; }

    public int IdganadoParienteFa { get; set; }

    public virtual Ganado IdganadoFaNavigation { get; set; } = null!;

    public virtual Ganado IdganadoParienteFaNavigation { get; set; } = null!;

    public virtual TipoParentesco IdtipoParentescoFaNavigation { get; set; } = null!;
}
