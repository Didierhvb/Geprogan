using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class Ganado
{
    public int Idganado { get; set; }

    public int Idfinca { get; set; }

    public int IdtipoGanado { get; set; }

    public string? MarcaGanado { get; set; }

    public string? Raza { get; set; }

    public DateOnly FechaNacimiento { get; set; }

    public string? Caracteristicas { get; set; }

    public string? NombreGanado { get; set; }

    public string Sexo { get; set; } = null!;

    public int? NumeroId { get; set; }

    public int? NumeroInventario { get; set; }

    public string? UrlImagen { get; set; }

    public virtual ICollection<CicloProduccion> CicloProduccions { get; set; } = new List<CicloProduccion>();

    public virtual ICollection<FallecimientoGanado> FallecimientoGanados { get; set; } = new List<FallecimientoGanado>();

    public virtual ICollection<FamiliaGanado> FamiliaGanadoIdganadoFaNavigations { get; set; } = new List<FamiliaGanado>();

    public virtual ICollection<FamiliaGanado> FamiliaGanadoIdganadoParienteFaNavigations { get; set; } = new List<FamiliaGanado>();

    public virtual Finca IdfincaNavigation { get; set; } = null!;

    public virtual TipoGanado IdtipoGanadoNavigation { get; set; } = null!;

    public virtual ICollection<MovimientoGanado> MovimientoGanados { get; set; } = new List<MovimientoGanado>();

    public virtual ICollection<Parto> PartoIdcriaPtNavigations { get; set; } = new List<Parto>();

    public virtual ICollection<Parto> PartoIdganadoPtNavigations { get; set; } = new List<Parto>();

    public virtual ICollection<Pesaje> Pesajes { get; set; } = new List<Pesaje>();

    public virtual ICollection<Produccion> Produccions { get; set; } = new List<Produccion>();

    public virtual ICollection<Sanidad> Sanidads { get; set; } = new List<Sanidad>();

    public virtual ICollection<SeguimientoReproduccion> SeguimientoReproduccions { get; set; } = new List<SeguimientoReproduccion>();

    public virtual ICollection<TrazabilidadGanado> TrazabilidadGanados { get; set; } = new List<TrazabilidadGanado>();
}
