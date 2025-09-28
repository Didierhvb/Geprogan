using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class TipoGanado
{
    public int IdtipoGanado { get; set; }

    public string NombreTg { get; set; } = null!;

    public virtual ICollection<Ganado> Ganados { get; set; } = new List<Ganado>();
}
