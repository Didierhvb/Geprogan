using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class Finca
{
    public int Idfinca { get; set; }

    public string NombreFinca { get; set; } = null!;

    public string Ubicacion { get; set; } = null!;

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public decimal Hectareas { get; set; }

    public int Propietario { get; set; }

    public virtual ICollection<FincaProduccion> FincaProduccions { get; set; } = new List<FincaProduccion>();

    public virtual ICollection<Ganado> Ganados { get; set; } = new List<Ganado>();

    public virtual Usuario PropietarioNavigation { get; set; } = null!;

    public virtual ICollection<UsuarioFinca> UsuarioFincas { get; set; } = new List<UsuarioFinca>();
}
