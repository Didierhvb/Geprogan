using Microsoft.ML.Data;

namespace GeproganAPP.DTOs
{
    public class DatosSaludAnimal
    {
        [LoadColumn(0)]
        public float IdGanado { get; set; }

        [LoadColumn(1)]
        public float PesoActual { get; set; }

        [LoadColumn(2)]
        public float PesoAnterior { get; set; }

        [LoadColumn(3)]
        public float CondicionCorporal { get; set; }

        [LoadColumn(4)]
        public float DiasDesdeUltimoPesaje { get; set; }

        [LoadColumn(5)]
        public float ProduccionPromedio { get; set; }

        [LoadColumn(6)]
        public float CantidadTratamientos { get; set; }

        [LoadColumn(7)]
        public float EdadEnDias { get; set; }
    }

    public class PrediccionAnomalíaSalud
    {
        [VectorType(8)]
        public float[] Caracteristicas { get; set; } = null!;

        [ColumnName("PredictedLabel")]
        public bool EsAnomalia { get; set; }

        [ColumnName("Score")]
        public float Puntuacion { get; set; }
    }

    public class AnalisisSaludAnimalDto
    {
        public int IdGanado { get; set; }
        public string? NombreGanado { get; set; }
        public bool EsAnomalia { get; set; }
        public float PuntuacionAnomalia { get; set; }
        public string NivelRiesgo { get; set; } = "Normal";
        public List<string> AlertasSalud { get; set; } = new List<string>();
        public DateTime FechaAnalisis { get; set; }
        public float? PesoActual { get; set; }
        public float? CondicionCorporal { get; set; }
        public float? ProduccionPromedio { get; set; }
    }

    public class MetricasSaludDto
    {
        public int TotalAnimales { get; set; }
        public int AnimalesConAnomalias { get; set; }
        public int AnimalesRiesgoAlto { get; set; }
        public int AnimalesRiesgoMedio { get; set; }
        public float PorcentajeAnomalias { get; set; }
        public List<AnalisisSaludAnimalDto> DetalleAnomalias { get; set; } = new List<AnalisisSaludAnimalDto>();
        public DateTime FechaAnalisis { get; set; }
    }
}