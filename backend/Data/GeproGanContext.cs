using System;
using System.Collections.Generic;
using GeproganAPP.Models;
using Microsoft.EntityFrameworkCore;

namespace GeproganAPP.Data;

public partial class GeproGanContext : DbContext
{
    public GeproGanContext()
    {
    }

    public GeproGanContext(DbContextOptions<GeproGanContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CicloProduccion> CicloProduccions { get; set; }

    public virtual DbSet<FallecimientoGanado> FallecimientoGanados { get; set; }

    public virtual DbSet<FamiliaGanado> FamiliaGanados { get; set; }

    public virtual DbSet<Finca> Fincas { get; set; }

    public virtual DbSet<FincaProduccion> FincaProduccions { get; set; }

    public virtual DbSet<Ganado> Ganados { get; set; }

    public virtual DbSet<MovimientoGanado> MovimientoGanados { get; set; }

    public virtual DbSet<Parto> Partos { get; set; }

    public virtual DbSet<Pesaje> Pesajes { get; set; }

    public virtual DbSet<Produccion> Produccions { get; set; }

    public virtual DbSet<ProductoSanidad> ProductoSanidads { get; set; }

    public virtual DbSet<Reporte> Reportes { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Sanidad> Sanidads { get; set; }

    public virtual DbSet<SeguimientoParto> SeguimientoPartos { get; set; }

    public virtual DbSet<SeguimientoReproduccion> SeguimientoReproduccions { get; set; }

    public virtual DbSet<TipoGanado> TipoGanados { get; set; }

    public virtual DbSet<TipoMovimiento> TipoMovimientos { get; set; }

    public virtual DbSet<TipoParentesco> TipoParentescos { get; set; }

    public virtual DbSet<TipoUsuario> TipoUsuarios { get; set; }

    public virtual DbSet<TrazabilidadGanado> TrazabilidadGanados { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuarioFinca> UsuarioFincas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CicloProduccion>(entity =>
        {
            entity.HasKey(e => e.Idciclo).HasName("PK__Ciclo_Pr__495052F8EB49192E");

            entity.ToTable("Ciclo_Produccion");

            entity.Property(e => e.Idciclo).HasColumnName("IDCiclo");
            entity.Property(e => e.CantidadTotalCp).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IdganadoCp).HasColumnName("IDGanadoCp");
            entity.Property(e => e.IdusuarioCp).HasColumnName("IDUsuarioCp");
            entity.Property(e => e.ObservacionesCp).HasMaxLength(600);

            entity.HasOne(d => d.IdganadoCpNavigation).WithMany(p => p.CicloProduccions)
                .HasForeignKey(d => d.IdganadoCp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CicloProduccion_Ganado");

            entity.HasOne(d => d.IdusuarioCpNavigation).WithMany(p => p.CicloProduccions)
                .HasForeignKey(d => d.IdusuarioCp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CicloProduccion_Usuario");
        });

        modelBuilder.Entity<FallecimientoGanado>(entity =>
        {
            entity.HasKey(e => e.IdfallecimientoGanado).HasName("PK__Fallecim__9BD225C8F2F1333F");

            entity.ToTable("Fallecimiento_Ganado");

            entity.Property(e => e.IdfallecimientoGanado).HasColumnName("IDFallecimientoGanado");
            entity.Property(e => e.CausaMg).HasMaxLength(500);
            entity.Property(e => e.IdganadoFg).HasColumnName("IDGanadoFg");
            entity.Property(e => e.IdusuarioFg).HasColumnName("IDUsuarioFg");

            entity.HasOne(d => d.IdganadoFgNavigation).WithMany(p => p.FallecimientoGanados)
                .HasForeignKey(d => d.IdganadoFg)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Fallecimiento_Ganado");

            entity.HasOne(d => d.IdusuarioFgNavigation).WithMany(p => p.FallecimientoGanados)
                .HasForeignKey(d => d.IdusuarioFg)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Fallecimiento_Usuario");
        });

        modelBuilder.Entity<FamiliaGanado>(entity =>
        {
            entity.HasKey(e => e.IdfamiliaGanado).HasName("PK__Familia___6B7BD123E23DED14");

            entity.ToTable("Familia_Ganado");

            entity.Property(e => e.IdfamiliaGanado).HasColumnName("IDFamiliaGanado");
            entity.Property(e => e.IdganadoFa).HasColumnName("IDGanadoFa");
            entity.Property(e => e.IdganadoParienteFa).HasColumnName("IDGanadoParienteFa");
            entity.Property(e => e.IdtipoParentescoFa).HasColumnName("IDTipoParentescoFa");

            entity.HasOne(d => d.IdganadoFaNavigation).WithMany(p => p.FamiliaGanadoIdganadoFaNavigations)
                .HasForeignKey(d => d.IdganadoFa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FamiliaGanado_Ganado");

            entity.HasOne(d => d.IdganadoParienteFaNavigation).WithMany(p => p.FamiliaGanadoIdganadoParienteFaNavigations)
                .HasForeignKey(d => d.IdganadoParienteFa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FamiliaGanado_Pariente");

            entity.HasOne(d => d.IdtipoParentescoFaNavigation).WithMany(p => p.FamiliaGanados)
                .HasForeignKey(d => d.IdtipoParentescoFa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FamiliaGanado_TipoParentesco");
        });

        modelBuilder.Entity<Finca>(entity =>
        {
            entity.HasKey(e => e.Idfinca).HasName("PK__Finca__49DA3ABC2761B884");

            entity.ToTable("Finca");

            entity.Property(e => e.Idfinca).HasColumnName("IDFinca");
            entity.Property(e => e.Hectareas).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Latitud).HasColumnType("decimal(10, 8)");
            entity.Property(e => e.Longitud).HasColumnType("decimal(11, 8)");
            entity.Property(e => e.NombreFinca).HasMaxLength(100);
            entity.Property(e => e.Ubicacion).HasMaxLength(255);

            entity.HasOne(d => d.PropietarioNavigation).WithMany(p => p.Fincas)
                .HasForeignKey(d => d.Propietario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Finca_Usuario");
        });

        modelBuilder.Entity<FincaProduccion>(entity =>
        {
            entity.HasKey(e => e.IdfincaProduccion).HasName("PK__Finca_Pr__B9060F5D390891DA");

            entity.ToTable("Finca_Produccion");

            entity.Property(e => e.IdfincaProduccion).HasColumnName("IDFincaProduccion");
            entity.Property(e => e.IdfincaFp).HasColumnName("IDFincaFp");
            entity.Property(e => e.TotalProducido).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdfincaFpNavigation).WithMany(p => p.FincaProduccions)
                .HasForeignKey(d => d.IdfincaFp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FincaProduccion_Finca");
        });

        modelBuilder.Entity<Ganado>(entity =>
        {
            entity.HasKey(e => e.Idganado).HasName("PK__Ganado__97B95CE6E6E2D3B3");

            entity.ToTable("Ganado");

            entity.HasIndex(e => e.NumeroId, "UQ__Ganado__C664E5CF811ADF4F").IsUnique();

            entity.Property(e => e.Idganado).HasColumnName("IDGanado");
            entity.Property(e => e.Caracteristicas).HasMaxLength(255);
            entity.Property(e => e.Idfinca).HasColumnName("IDFinca");
            entity.Property(e => e.IdtipoGanado).HasColumnName("IDTipoGanado");
            entity.Property(e => e.MarcaGanado).HasMaxLength(255);
            entity.Property(e => e.NombreGanado).HasMaxLength(45);
            entity.Property(e => e.Raza).HasMaxLength(50);
            entity.Property(e => e.Sexo).HasMaxLength(45);
            entity.Property(e => e.UrlImagen).HasMaxLength(255);

            entity.HasOne(d => d.IdfincaNavigation).WithMany(p => p.Ganados)
                .HasForeignKey(d => d.Idfinca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ganado_Finca");

            entity.HasOne(d => d.IdtipoGanadoNavigation).WithMany(p => p.Ganados)
                .HasForeignKey(d => d.IdtipoGanado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ganado_TipoGanado");
        });

        modelBuilder.Entity<MovimientoGanado>(entity =>
        {
            entity.HasKey(e => e.IdmovimientoGanado).HasName("PK__Movimien__D51A287954E50970");

            entity.ToTable("Movimiento_Ganado");

            entity.Property(e => e.IdmovimientoGanado).HasColumnName("IDMovimientoGanado");
            entity.Property(e => e.IdfincaDestino).HasColumnName("IDFincaDestino");
            entity.Property(e => e.IdfincaOrigne).HasColumnName("IDFincaOrigne");
            entity.Property(e => e.IdganadoMg).HasColumnName("IDGanadoMg");
            entity.Property(e => e.IdtipoMovimientoMg).HasColumnName("IDTipoMovimientoMg");
            entity.Property(e => e.IdusuarioMg).HasColumnName("IDUsuarioMg");
            entity.Property(e => e.ObservacionesMg).HasMaxLength(600);
            entity.Property(e => e.ValorMg).HasMaxLength(45);

            entity.HasOne(d => d.IdganadoMgNavigation).WithMany(p => p.MovimientoGanados)
                .HasForeignKey(d => d.IdganadoMg)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Movimiento_Ganado");

            entity.HasOne(d => d.IdtipoMovimientoMgNavigation).WithMany(p => p.MovimientoGanados)
                .HasForeignKey(d => d.IdtipoMovimientoMg)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Movimiento_TipoMovimiento");

            entity.HasOne(d => d.IdusuarioMgNavigation).WithMany(p => p.MovimientoGanados)
                .HasForeignKey(d => d.IdusuarioMg)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Movimiento_Usuario");
        });

        modelBuilder.Entity<Parto>(entity =>
        {
            entity.HasKey(e => e.Idparto).HasName("PK__Parto__DF0F0E37596F0A19");

            entity.ToTable("Parto");

            entity.Property(e => e.Idparto).HasColumnName("IDParto");
            entity.Property(e => e.IdcriaPt).HasColumnName("IDCriaPt");
            entity.Property(e => e.IdganadoPt).HasColumnName("IDGanadoPt");
            entity.Property(e => e.IdusuarioPt).HasColumnName("IDUsuarioPt");
            entity.Property(e => e.ObservacionesPt).HasMaxLength(600);

            entity.HasOne(d => d.IdcriaPtNavigation).WithMany(p => p.PartoIdcriaPtNavigations)
                .HasForeignKey(d => d.IdcriaPt)
                .HasConstraintName("FK_Parto_Cria");

            entity.HasOne(d => d.IdganadoPtNavigation).WithMany(p => p.PartoIdganadoPtNavigations)
                .HasForeignKey(d => d.IdganadoPt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Parto_Ganado");

            entity.HasOne(d => d.IdusuarioPtNavigation).WithMany(p => p.Partos)
                .HasForeignKey(d => d.IdusuarioPt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Parto_Usuario");
        });

        modelBuilder.Entity<Pesaje>(entity =>
        {
            entity.HasKey(e => e.Idpesaje).HasName("PK__Pesaje__8BBFCCBD891C797C");

            entity.ToTable("Pesaje");

            entity.Property(e => e.Idpesaje).HasColumnName("IDPesaje");
            entity.Property(e => e.IdganadoPj).HasColumnName("IDGanadoPj");
            entity.Property(e => e.IdusuarioPj).HasColumnName("IDUsuarioPj");
            entity.Property(e => e.Observaciones).HasMaxLength(600);

            entity.HasOne(d => d.IdganadoPjNavigation).WithMany(p => p.Pesajes)
                .HasForeignKey(d => d.IdganadoPj)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pesaje_Ganado");

            entity.HasOne(d => d.IdusuarioPjNavigation).WithMany(p => p.Pesajes)
                .HasForeignKey(d => d.IdusuarioPj)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pesaje_Usuario");
        });

        modelBuilder.Entity<Produccion>(entity =>
        {
            entity.HasKey(e => e.Idproduccion).HasName("PK__Producci__D8EF32FE7065F372");

            entity.ToTable("Produccion");

            entity.Property(e => e.Idproduccion).HasColumnName("IDProduccion");
            entity.Property(e => e.CantidadPr).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.HorarioPr).HasMaxLength(205);
            entity.Property(e => e.IdganadoPr).HasColumnName("IDGanadoPr");
            entity.Property(e => e.IdusuarioPr).HasColumnName("IDUsuarioPr");
            entity.Property(e => e.ObservacionesPr).HasMaxLength(600);

            entity.HasOne(d => d.IdganadoPrNavigation).WithMany(p => p.Produccions)
                .HasForeignKey(d => d.IdganadoPr)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Produccion_Ganado");

            entity.HasOne(d => d.IdusuarioPrNavigation).WithMany(p => p.Produccions)
                .HasForeignKey(d => d.IdusuarioPr)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Produccion_Usuario");
        });

        modelBuilder.Entity<ProductoSanidad>(entity =>
        {
            entity.HasKey(e => e.IdproductoSanidad).HasName("PK__Producto__BA1375C8B4B7D39E");

            entity.ToTable("Producto_Sanidad");

            entity.Property(e => e.IdproductoSanidad).HasColumnName("IDProductoSanidad");
            entity.Property(e => e.ContenidoMl).HasMaxLength(45);
            entity.Property(e => e.NombreProductoPs).HasMaxLength(100);
        });

        modelBuilder.Entity<Reporte>(entity =>
        {
            entity.HasKey(e => e.Idreporte).HasName("PK__Reporte__F69C2DEC1D96481B");

            entity.ToTable("Reporte");

            entity.Property(e => e.Idreporte).HasColumnName("IDReporte");
            entity.Property(e => e.Descripcion).HasMaxLength(600);
            entity.Property(e => e.IdusuarioRp).HasColumnName("IDUsuarioRp");

            entity.HasOne(d => d.IdusuarioRpNavigation).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.IdusuarioRp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reporte_Usuario");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.Idrol).HasName("PK__Rol__A681ACB6A8C90825");

            entity.ToTable("Rol");

            entity.Property(e => e.Idrol).HasColumnName("IDRol");
            entity.Property(e => e.NombreRol).HasMaxLength(50);
        });

        modelBuilder.Entity<Sanidad>(entity =>
        {
            entity.HasKey(e => e.Idsanidad).HasName("PK__Sanidad__4AFD2BDC412FB579");

            entity.ToTable("Sanidad");

            entity.Property(e => e.Idsanidad).HasColumnName("IDSanidad");
            entity.Property(e => e.DescripcionSn).HasMaxLength(500);
            entity.Property(e => e.IdganadoSn).HasColumnName("IDGanadoSn");
            entity.Property(e => e.IdusuarioSn).HasColumnName("IDUsuarioSn");
            entity.Property(e => e.ObservacionesSn).HasMaxLength(600);

            entity.HasOne(d => d.IdganadoSnNavigation).WithMany(p => p.Sanidads)
                .HasForeignKey(d => d.IdganadoSn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sanidad_Ganado");

            entity.HasOne(d => d.IdusuarioSnNavigation).WithMany(p => p.Sanidads)
                .HasForeignKey(d => d.IdusuarioSn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sanidad_Usuario");

            entity.HasOne(d => d.ProductoSnNavigation).WithMany(p => p.Sanidads)
                .HasForeignKey(d => d.ProductoSn)
                .HasConstraintName("FK_Sanidad_Producto");
        });

        modelBuilder.Entity<SeguimientoParto>(entity =>
        {
            entity.HasKey(e => e.IdseguimientoParto).HasName("PK__Seguimie__7B88180EA41F0195");

            entity.ToTable("Seguimiento_Parto");

            entity.Property(e => e.IdseguimientoParto).HasColumnName("IDSeguimientoParto");
            entity.Property(e => e.IdpartoSp).HasColumnName("IDPartoSp");
            entity.Property(e => e.IdusuarioSp).HasColumnName("IDUsuarioSp");
            entity.Property(e => e.ProduccionTotalSp).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdpartoSpNavigation).WithMany(p => p.SeguimientoPartos)
                .HasForeignKey(d => d.IdpartoSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeguimientoParto_Parto");

            entity.HasOne(d => d.IdusuarioSpNavigation).WithMany(p => p.SeguimientoPartos)
                .HasForeignKey(d => d.IdusuarioSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeguimientoParto_Usuario");
        });

        modelBuilder.Entity<SeguimientoReproduccion>(entity =>
        {
            entity.HasKey(e => e.IdseguimientoReproduccion).HasName("PK__Seguimie__7E4B04FD887466C5");

            entity.ToTable("Seguimiento_Reproduccion");

            entity.Property(e => e.IdseguimientoReproduccion).HasColumnName("IDSeguimientoReproduccion");
            entity.Property(e => e.IdganadoSr).HasColumnName("IDGanadoSr");
            entity.Property(e => e.IdusuarioSr).HasColumnName("IDUsuarioSr");
            entity.Property(e => e.ObservacionesSr).HasMaxLength(500);
            entity.Property(e => e.Palpacion).HasMaxLength(255);
            entity.Property(e => e.Servicio).HasMaxLength(255);

            entity.HasOne(d => d.IdganadoSrNavigation).WithMany(p => p.SeguimientoReproduccions)
                .HasForeignKey(d => d.IdganadoSr)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeguimientoReproduccion_Ganado");

            entity.HasOne(d => d.IdusuarioSrNavigation).WithMany(p => p.SeguimientoReproduccions)
                .HasForeignKey(d => d.IdusuarioSr)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeguimientoReproduccion_Usuario");
        });

        modelBuilder.Entity<TipoGanado>(entity =>
        {
            entity.HasKey(e => e.IdtipoGanado).HasName("PK__Tipo_Gan__B7F6EEF10394A57C");

            entity.ToTable("Tipo_Ganado");

            entity.HasIndex(e => e.NombreTg, "UQ__Tipo_Gan__E6939C096EF1293A").IsUnique();

            entity.Property(e => e.IdtipoGanado).HasColumnName("IDTipoGanado");
            entity.Property(e => e.NombreTg).HasMaxLength(50);
        });

        modelBuilder.Entity<TipoMovimiento>(entity =>
        {
            entity.HasKey(e => e.IdtipoMovimiento).HasName("PK__Tipo_Mov__7647A89AF6C37F0C");

            entity.ToTable("Tipo_Movimiento");

            entity.Property(e => e.IdtipoMovimiento).HasColumnName("IDTipoMovimiento");
            entity.Property(e => e.NombreMovimientoTm).HasMaxLength(100);
        });

        modelBuilder.Entity<TipoParentesco>(entity =>
        {
            entity.HasKey(e => e.IdtipoParentesco).HasName("PK__Tipo_Par__0175AF49988C0A1C");

            entity.ToTable("Tipo_Parentesco");

            entity.Property(e => e.IdtipoParentesco).HasColumnName("IDTipoParentesco");
            entity.Property(e => e.NombreTp).HasMaxLength(45);
        });

        modelBuilder.Entity<TipoUsuario>(entity =>
        {
            entity.HasKey(e => e.IdtipoUsuario).HasName("PK__Tipo_Usu__53289754E4170224");

            entity.ToTable("Tipo_Usuario");

            entity.Property(e => e.IdtipoUsuario).HasColumnName("IDTipoUsuario");
            entity.Property(e => e.TipoUsuarioTu).HasMaxLength(50);
        });

        modelBuilder.Entity<TrazabilidadGanado>(entity =>
        {
            entity.HasKey(e => e.IdtrazabilidadGanado).HasName("PK__Trazabil__2102B3468BE50B10");

            entity.ToTable("Trazabilidad_Ganado");

            entity.Property(e => e.IdtrazabilidadGanado).HasColumnName("IDTrazabilidadGanado");
            entity.Property(e => e.CaracteristicasTg).HasMaxLength(255);
            entity.Property(e => e.ComentarioTg).HasMaxLength(500);
            entity.Property(e => e.IdganadoTg).HasColumnName("IDGanadoTg");
            entity.Property(e => e.IdusuarioTg).HasColumnName("IDUsuarioTg");
            entity.Property(e => e.MarcaGanadoTg).HasMaxLength(45);
            entity.Property(e => e.NombreGanadoTg).HasMaxLength(255);
            entity.Property(e => e.NumeroIdTg).HasMaxLength(45);
            entity.Property(e => e.NumeroInventarioTg).HasMaxLength(45);
            entity.Property(e => e.RazaTg).HasMaxLength(255);
            entity.Property(e => e.SexoTg).HasMaxLength(45);
            entity.Property(e => e.UrlImagenTg).HasMaxLength(255);

            entity.HasOne(d => d.IdganadoTgNavigation).WithMany(p => p.TrazabilidadGanados)
                .HasForeignKey(d => d.IdganadoTg)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trazabilidad_Ganado");

            entity.HasOne(d => d.IdusuarioTgNavigation).WithMany(p => p.TrazabilidadGanados)
                .HasForeignKey(d => d.IdusuarioTg)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trazabilidad_Usuario");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Idusuario).HasName("PK__Usuario__523111691369D741");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.EmailUr, "UQ__Usuario__7ED9BB61B3F130FA").IsUnique();

            entity.Property(e => e.Idusuario)
                .ValueGeneratedNever()
                .HasColumnName("IDUsuario");
            entity.Property(e => e.ApellidoUr).HasMaxLength(50);
            entity.Property(e => e.Contrasena).HasMaxLength(255);
            entity.Property(e => e.EmailUr).HasMaxLength(100);
            entity.Property(e => e.IdrolUr).HasColumnName("IDRolUr");
            entity.Property(e => e.NombreUr).HasMaxLength(50);
            entity.Property(e => e.TelefonoUr).HasMaxLength(10);
            entity.Property(e => e.TipoIdentificacion).HasMaxLength(50);
            entity.Property(e => e.UrlImageUr).HasMaxLength(205);

            entity.HasOne(d => d.IdrolUrNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdrolUr)
                .HasConstraintName("FK_Usuario_Rol");
        });

        modelBuilder.Entity<UsuarioFinca>(entity =>
        {
            entity.HasKey(e => e.IdusuarioFinca).HasName("PK__Usuario___4CE417938D375EB0");

            entity.ToTable("Usuario_Finca");

            entity.Property(e => e.IdusuarioFinca).HasColumnName("IDUsuarioFinca");
            entity.Property(e => e.IdfincaUf).HasColumnName("IDFincaUf");
            entity.Property(e => e.IdusuarioUf).HasColumnName("IDUsuarioUf");

            entity.HasOne(d => d.IdfincaUfNavigation).WithMany(p => p.UsuarioFincas)
                .HasForeignKey(d => d.IdfincaUf)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuarioFinca_Finca");

            entity.HasOne(d => d.IdusuarioUfNavigation).WithMany(p => p.UsuarioFincas)
                .HasForeignKey(d => d.IdusuarioUf)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuarioFinca_Usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
