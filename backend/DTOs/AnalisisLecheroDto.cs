using Microsoft.ML.Data;

namespace GeproganAPP.DTOs
{
    public class DatosProduccionLechera
    {
        [LoadColumn(0)]
        public float IdGanado { get; set; }

        [LoadColumn(1)]
        public float EdadEnDias { get; set; }

        [LoadColumn(2)]
        public float DiasEnLactancia { get; set; }

        [LoadColumn(3)]
        public float ProduccionPromedioDiaria { get; set; }

        [LoadColumn(4)]
        public float ProduccionUltimos7Dias { get; set; }

        [LoadColumn(5)]
        public float ProduccionUltimos30Dias { get; set; }

        [LoadColumn(6)]
        public float PesoActual { get; set; }

        [LoadColumn(7)]
        public float CondicionCorporal { get; set; }

        [LoadColumn(8)]
        public float NumeroPartos { get; set; }

        [LoadColumn(9)]
        public float TemporadaAnio { get; set; } // 1-4 (primavera, verano, otoño, invierno)

        [LoadColumn(10)]
        public float ProduccionAnteriorMismaPeriodo { get; set; }
    }

    public class PrediccionProduccionLechera
    {
        [ColumnName("Score")]
        public float ProduccionPredicha { get; set; }

        [VectorType(11)]
        public float[] Caracteristicas { get; set; } = null!;
    }

    public class DatosCalidadLeche
    {
        [LoadColumn(0)]
        public float CantidadDiaria { get; set; }

        [LoadColumn(1)]
        public float FrecuenciaOrdeño { get; set; }

        [LoadColumn(2)]
        public float HorasEntreDosis { get; set; }

        [LoadColumn(3)]
        public float CondicionCorporalVaca { get; set; }

        [LoadColumn(4)]
        public float EdadVaca { get; set; }

        [LoadColumn(5)]
        public float DiasEnLactancia { get; set; }

        [LoadColumn(6)]
        public float TemporadaProduccion { get; set; }
    }

    public class PrediccionCalidadLeche
    {
        [ColumnName("PredictedLabel")]
        public uint CategoriaCalidad { get; set; } // 0: Baja, 1: Media, 2: Alta

        [ColumnName("Score")]
        public float[] Probabilidades { get; set; } = null!;
    }

    public class AnalisisProduccionDto
    {
        public int IdGanado { get; set; }
        public string? NombreGanado { get; set; }
        public float ProduccionActual { get; set; }
        public float ProduccionPredicha { get; set; }
        public float DiferenciaPorcentual { get; set; }
        public string TendenciaProduccion { get; set; } = "Estable";
        public string ClasificacionCalidad { get; set; } = "Media";
        public float ProbabilidadCalidadAlta { get; set; }
        public List<string> Recomendaciones { get; set; } = new List<string>();
        public DateTime FechaAnalisis { get; set; }
        public DatosProduccionDetalle Detalles { get; set; } = new DatosProduccionDetalle();
    }

    public class DatosProduccionDetalle
    {
        public float ProduccionPromedio7Dias { get; set; }
        public float ProduccionPromedio30Dias { get; set; }
        public int DiasEnLactancia { get; set; }
        public int NumeroPartos { get; set; }
        public float CondicionCorporal { get; set; }
        public string TemporadaActual { get; set; } = "Primavera";
    }

    public class MetricasProduccionDto
    {
        public int TotalVacasLecheras { get; set; }
        public float ProduccionTotalDiaria { get; set; }
        public float ProduccionPromedioPorVaca { get; set; }
        public float ProduccionPredichaSemana { get; set; }
        public float ProduccionPredichaMes { get; set; }
        public int VacasCalidadAlta { get; set; }
        public int VacasCalidadMedia { get; set; }
        public int VacasCalidadBaja { get; set; }
        public float PorcentajeCalidadAlta { get; set; }
        public List<AnalisisProduccionDto> DetalleVacas { get; set; } = new List<AnalisisProduccionDto>();
        public DateTime FechaAnalisis { get; set; }
        public List<string> RecomendacionesGenerales { get; set; } = new List<string>();
    }

    public class OptimizacionProduccionDto
    {
        public int IdGanado { get; set; }
        public string? NombreGanado { get; set; }
        public float ProduccionActual { get; set; }
        public float ProduccionOptima { get; set; }
        public float PotencialMejora { get; set; }
        public List<AccionOptimizacion> AccionesRecomendadas { get; set; } = new List<AccionOptimizacion>();
        public int PrioridadOptimizacion { get; set; } // 1: Alta, 2: Media, 3: Baja
    }

    public class AccionOptimizacion
    {
        public string Accion { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public float ImpactoEstimado { get; set; } // Litros adicionales esperados
        public string Categoria { get; set; } = ""; // Alimentación, Manejo, Salud, etc.
    }

    public class TendenciaProduccionDto
    {
        public DateTime Fecha { get; set; }
        public float ProduccionReal { get; set; }
        public float ProduccionPredicha { get; set; }
        public float DiferenciaAbsoluta { get; set; }
        public int NumeroVacas { get; set; }
    }

    public class ReporteProduccionDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public MetricasProduccionDto Metricas { get; set; } = new MetricasProduccionDto();
        public List<TendenciaProduccionDto> Tendencias { get; set; } = new List<TendenciaProduccionDto>();
        public List<OptimizacionProduccionDto> Optimizaciones { get; set; } = new List<OptimizacionProduccionDto>();
        public float PrecisionModelo { get; set; }
        public int TotalPredicciones { get; set; }
        public float ErrorPromedioAbsoluto { get; set; }
    }
}