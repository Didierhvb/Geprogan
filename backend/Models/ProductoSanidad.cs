using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class ProductoSanidad
{
    public int IdproductoSanidad { get; set; }

    public string? NombreProductoPs { get; set; }

    public string? ContenidoMl { get; set; }

    public virtual ICollection<Sanidad> Sanidads { get; set; } = new List<Sanidad>();
}
