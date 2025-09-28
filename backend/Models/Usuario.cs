using System;
using System.Collections.Generic;

namespace GeproganAPP.Models;

public partial class Usuario
{
    public int Idusuario { get; set; }

    public int? IdrolUr { get; set; }

    public string TipoIdentificacion { get; set; } = null!;

    public string NombreUr { get; set; } = null!;

    public string ApellidoUr { get; set; } = null!;

    public string EmailUr { get; set; } = null!;

    public string TelefonoUr { get; set; } = null!;

    public string Contrasena { get; set; } = null!;

    public string UrlImageUr { get; set; } = null!;

    public virtual ICollection<CicloProduccion> CicloProduccions { get; set; } = new List<CicloProduccion>();

    public virtual ICollection<FallecimientoGanado> FallecimientoGanados { get; set; } = new List<FallecimientoGanado>();

    public virtual ICollection<Finca> Fincas { get; set; } = new List<Finca>();

    public virtual Rol? IdrolUrNavigation { get; set; }

    public virtual ICollection<MovimientoGanado> MovimientoGanados { get; set; } = new List<MovimientoGanado>();

    public virtual ICollection<Parto> Partos { get; set; } = new List<Parto>();

    public virtual ICollection<Pesaje> Pesajes { get; set; } = new List<Pesaje>();

    public virtual ICollection<Produccion> Produccions { get; set; } = new List<Produccion>();

    public virtual ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();

    public virtual ICollection<Sanidad> Sanidads { get; set; } = new List<Sanidad>();

    public virtual ICollection<SeguimientoParto> SeguimientoPartos { get; set; } = new List<SeguimientoParto>();

    public virtual ICollection<SeguimientoReproduccion> SeguimientoReproduccions { get; set; } = new List<SeguimientoReproduccion>();

    public virtual ICollection<TrazabilidadGanado> TrazabilidadGanados { get; set; } = new List<TrazabilidadGanado>();

    public virtual ICollection<UsuarioFinca> UsuarioFincas { get; set; } = new List<UsuarioFinca>();
}
