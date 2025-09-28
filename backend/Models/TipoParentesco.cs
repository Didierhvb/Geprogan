using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class TipoParentesco
{
    public int IdtipoParentesco { get; set; }

    public string NombreTp { get; set; } = null!;

    public virtual ICollection<FamiliaGanado> FamiliaGanados { get; set; } = new List<FamiliaGanado>();
}
