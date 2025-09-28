using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class Rol
{
    public int Idrol { get; set; }

    public string NombreRol { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
