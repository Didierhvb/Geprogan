using GeproganAPP.DTOs;
using GeproganAPP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeproganAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalisisSaludController : ControllerBase
    {
        private readonly IServicioDeteccionAnomalias _servicioDeteccion;

        public AnalisisSaludController(IServicioDeteccionAnomalias servicioDeteccion)
        {
            _servicioDeteccion = servicioDeteccion;
        }

        [HttpPost("entrenar-modelo")]
        public async Task<IActionResult> EntrenarModelo()
        {
            try
            {
                var exito = await _servicioDeteccion.EntrenarModeloAsync();

                if (exito)
                {
                    return Ok(new { mensaje = "Modelo entrenado exitosamente", fecha = DateTime.Now });
                }
                else
                {
                    return BadRequest(new { mensaje = "Error al entrenar el modelo" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpGet("analizar-animal/{idGanado}")]
        public async Task<IActionResult> AnalizarAnimal(int idGanado)
        {
            try
            {
                var analisis = await _servicioDeteccion.AnalizarSaludAnimalAsync(idGanado);
                return Ok(analisis);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpGet("detectar-anomalias")]
        public async Task<IActionResult> DetectarAnomalias()
        {
            try
            {
                var anomalias = await _servicioDeteccion.DetectarAnomaliasMasivasAsync();
                return Ok(new
                {
                    totalAnomalias = anomalias.Count,
                    fechaAnalisis = DateTime.Now,
                    anomalias = anomalias
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpGet("metricas-generales")]
        public async Task<IActionResult> ObtenerMetricasGenerales()
        {
            try
            {
                var metricas = await _servicioDeteccion.ObtenerMetricasGeneralesAsync();
                return Ok(metricas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpGet("resumen-salud")]
        public async Task<IActionResult> ObtenerResumenSalud()
        {
            try
            {
                var metricas = await _servicioDeteccion.ObtenerMetricasGeneralesAsync();

                var resumen = new
                {
                    estadoGeneral = metricas.PorcentajeAnomalias < 10 ? "Excelente" :
                                   metricas.PorcentajeAnomalias < 20 ? "Bueno" :
                                   metricas.PorcentajeAnomalias < 30 ? "Regular" : "Crítico",
                    totalAnimales = metricas.TotalAnimales,
                    animalesSanos = metricas.TotalAnimales - metricas.AnimalesConAnomalias,
                    animalesConAnomalias = metricas.AnimalesConAnomalias,
                    porcentajeAnomalias = Math.Round(metricas.PorcentajeAnomalias, 2),
                    distribicionRiesgo = new
                    {
                        alto = metricas.AnimalesRiesgoAlto,
                        medio = metricas.AnimalesRiesgoMedio,
                        bajo = metricas.AnimalesConAnomalias - (metricas.AnimalesRiesgoAlto + metricas.AnimalesRiesgoMedio)
                    },
                    fechaAnalisis = metricas.FechaAnalisis,
                    recomendaciones = GenerarRecomendaciones(metricas)
                };

                return Ok(resumen);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpGet("alertas-urgentes")]
        public async Task<IActionResult> ObtenerAlertasUrgentes()
        {
            try
            {
                var anomalias = await _servicioDeteccion.DetectarAnomaliasMasivasAsync();
                var alertasUrgentes = anomalias
                    .Where(a => a.NivelRiesgo == "Alto")
                    .OrderByDescending(a => a.PuntuacionAnomalia)
                    .Take(10)
                    .Select(a => new
                    {
                        idGanado = a.IdGanado,
                        nombreGanado = a.NombreGanado ?? $"Animal #{a.IdGanado}",
                        puntuacionRiesgo = Math.Round(a.PuntuacionAnomalia, 3),
                        alertas = a.AlertasSalud,
                        requiereAtencionInmediata = a.PuntuacionAnomalia > 0.8,
                        fechaDeteccion = a.FechaAnalisis
                    })
                    .ToList();

                return Ok(new
                {
                    totalAlertas = alertasUrgentes.Count,
                    alertasUrgentes = alertasUrgentes,
                    fechaConsulta = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        private List<string> GenerarRecomendaciones(MetricasSaludDto metricas)
        {
            var recomendaciones = new List<string>();

            if (metricas.PorcentajeAnomalias > 20)
            {
                recomendaciones.Add("Se recomienda revisar las condiciones generales del hato");
                recomendaciones.Add("Evaluar la alimentación y suplementación nutricional");
            }

            if (metricas.AnimalesRiesgoAlto > 0)
            {
                recomendaciones.Add($"Atención veterinaria inmediata para {metricas.AnimalesRiesgoAlto} animal(es) de alto riesgo");
            }

            if (metricas.AnimalesRiesgoMedio > 5)
            {
                recomendaciones.Add("Implementar programa de monitoreo intensivo para animales de riesgo medio");
            }

            if (metricas.PorcentajeAnomalias < 5)
            {
                recomendaciones.Add("El hato presenta excelente estado de salud general");
                recomendaciones.Add("Mantener las prácticas actuales de manejo");
            }

            return recomendaciones;
        }
    }
}