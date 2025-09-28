using GeproganAPP.DTOs;
using GeproganAPP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeproganAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalisisLecheroController : ControllerBase
    {
        private readonly IServicioAnalisisLechero _servicioAnalisis;

        public AnalisisLecheroController(IServicioAnalisisLechero servicioAnalisis)
        {
            _servicioAnalisis = servicioAnalisis;
        }

        [HttpPost("entrenar-modelos")]
        public async Task<IActionResult> EntrenarModelos()
        {
            try
            {
                var exito = await _servicioAnalisis.EntrenarModelosAsync();

                if (exito)
                {
                    return Ok(new { mensaje = "Modelos de análisis lechero entrenados exitosamente", fecha = DateTime.Now });
                }
                else
                {
                    return BadRequest(new { mensaje = "Error al entrenar los modelos de análisis lechero" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpGet("analizar-vaca/{idGanado}")]
        public async Task<IActionResult> AnalizarProduccionVaca(int idGanado)
        {
            try
            {
                var analisis = await _servicioAnalisis.AnalizarProduccionVacaAsync(idGanado);
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

        [HttpGet("metricas-generales")]
        public async Task<IActionResult> ObtenerMetricasGenerales()
        {
            try
            {
                var metricas = await _servicioAnalisis.ObtenerMetricasGeneralesAsync();
                return Ok(metricas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpGet("optimizaciones")]
        public async Task<IActionResult> ObtenerOptimizaciones()
        {
            try
            {
                var optimizaciones = await _servicioAnalisis.ObtenerOptimizacionesAsync();
                return Ok(new
                {
                    totalOptimizaciones = optimizaciones.Count,
                    optimizacionesPrioridadAlta = optimizaciones.Count(o => o.PrioridadOptimizacion == 1),
                    optimizacionesPrioridadMedia = optimizaciones.Count(o => o.PrioridadOptimizacion == 2),
                    optimizacionesPrioridadBaja = optimizaciones.Count(o => o.PrioridadOptimizacion == 3),
                    potencialMejoraTotal = optimizaciones.Sum(o => o.PotencialMejora),
                    fechaAnalisis = DateTime.Now,
                    optimizaciones = optimizaciones
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpGet("tendencias")]
        public async Task<IActionResult> ObtenerTendencias([FromQuery] int dias = 30)
        {
            try
            {
                if (dias < 1 || dias > 365)
                {
                    return BadRequest(new { mensaje = "El número de días debe estar entre 1 y 365" });
                }

                var tendencias = await _servicioAnalisis.ObtenerTendenciasAsync(dias);
                return Ok(new
                {
                    periodo = $"Últimos {dias} días",
                    fechaInicio = DateTime.Now.AddDays(-dias),
                    fechaFin = DateTime.Now,
                    totalDias = tendencias.Count,
                    produccionPromedioReal = tendencias.Average(t => t.ProduccionReal),
                    produccionPromedioPredicha = tendencias.Average(t => t.ProduccionPredicha),
                    errorPromedioAbsoluto = tendencias.Average(t => t.DiferenciaAbsoluta),
                    tendencias = tendencias
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpGet("reporte-produccion")]
        public async Task<IActionResult> GenerarReporteProduccion(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                var inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
                var fin = fechaFin ?? DateTime.Now;

                if (inicio > fin)
                {
                    return BadRequest(new { mensaje = "La fecha de inicio no puede ser posterior a la fecha de fin" });
                }

                if ((fin - inicio).TotalDays > 365)
                {
                    return BadRequest(new { mensaje = "El rango de fechas no puede exceder 365 días" });
                }

                var reporte = await _servicioAnalisis.GenerarReporteProduccionAsync(inicio, fin);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> ObtenerDashboard()
        {
            try
            {
                // Obtener datos para el dashboard principal
                var metricas = await _servicioAnalisis.ObtenerMetricasGeneralesAsync();
                var tendencias = await _servicioAnalisis.ObtenerTendenciasAsync(7); // Última semana
                var optimizaciones = await _servicioAnalisis.ObtenerOptimizacionesAsync();

                var dashboard = new
                {
                    resumenGeneral = new
                    {
                        totalVacasLecheras = metricas.TotalVacasLecheras,
                        produccionTotalDiaria = Math.Round(metricas.ProduccionTotalDiaria, 2),
                        produccionPromedioPorVaca = Math.Round(metricas.ProduccionPromedioPorVaca, 2),
                        porcentajeCalidadAlta = Math.Round(metricas.PorcentajeCalidadAlta, 1),
                        estadoGeneral = DeterminarEstadoGeneral(metricas)
                    },
                    distribucionCalidad = new
                    {
                        alta = metricas.VacasCalidadAlta,
                        media = metricas.VacasCalidadMedia,
                        baja = metricas.VacasCalidadBaja,
                        porcentajes = new
                        {
                            alta = Math.Round(metricas.PorcentajeCalidadAlta, 1),
                            media = metricas.TotalVacasLecheras > 0 ? Math.Round((float)metricas.VacasCalidadMedia / metricas.TotalVacasLecheras * 100, 1) : 0,
                            baja = metricas.TotalVacasLecheras > 0 ? Math.Round((float)metricas.VacasCalidadBaja / metricas.TotalVacasLecheras * 100, 1) : 0
                        }
                    },
                    tendenciaReciente = new
                    {
                        dias = tendencias.Count,
                        tendencia = DeterminarTendenciaReciente(tendencias),
                        variacionPorcentual = CalcularVariacionPorcentual(tendencias),
                        datos = tendencias.TakeLast(7).Select(t => new
                        {
                            fecha = t.Fecha.ToString("dd/MM"),
                            produccionReal = Math.Round(t.ProduccionReal, 2),
                            produccionPredicha = Math.Round(t.ProduccionPredicha, 2)
                        })
                    },
                    optimizacionesPrioritarias = optimizaciones
                        .Where(o => o.PrioridadOptimizacion == 1)
                        .Take(5)
                        .Select(o => new
                        {
                            idGanado = o.IdGanado,
                            nombreGanado = o.NombreGanado ?? $"Vaca #{o.IdGanado}",
                            produccionActual = Math.Round(o.ProduccionActual, 2),
                            potencialMejora = Math.Round(o.PotencialMejora, 2),
                            porcentajeMejora = o.ProduccionActual > 0 ? Math.Round((o.PotencialMejora / o.ProduccionActual) * 100, 1) : 0,
                            accionPrincipal = o.AccionesRecomendadas.FirstOrDefault()?.Accion ?? "Evaluar condiciones"
                        }),
                    alertasProduccion = GenerarAlertasProduccion(metricas, tendencias),
                    recomendacionesPrioritarias = metricas.RecomendacionesGenerales.Take(3).ToList(),
                    fechaActualizacion = DateTime.Now
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpGet("prediccion-semanal")]
        public async Task<IActionResult> ObtenerPrediccionSemanal()
        {
            try
            {
                var metricas = await _servicioAnalisis.ObtenerMetricasGeneralesAsync();
                var tendencias = await _servicioAnalisis.ObtenerTendenciasAsync(30);

                // Calcular predicción para los próximos 7 días
                var prediccionSemanal = new List<object>();
                var produccionBase = tendencias.LastOrDefault()?.ProduccionReal ?? metricas.ProduccionTotalDiaria;

                for (int i = 1; i <= 7; i++)
                {
                    var fecha = DateTime.Now.AddDays(i);
                    var factorEstacional = ObtenerFactorEstacional(fecha);
                    var factorTendencia = ObtenerFactorTendencia(tendencias);

                    var produccionPredicha = produccionBase * factorEstacional * factorTendencia;

                    prediccionSemanal.Add(new
                    {
                        fecha = fecha.ToString("dd/MM/yyyy"),
                        diaSemana = fecha.ToString("dddd", new System.Globalization.CultureInfo("es-ES")),
                        produccionPredicha = Math.Round(produccionPredicha, 2),
                        confianza = CalcularConfianza(i), // La confianza disminuye con días más lejanos
                        factores = new
                        {
                            estacional = Math.Round(factorEstacional, 3),
                            tendencia = Math.Round(factorTendencia, 3)
                        }
                    });
                }

                return Ok(new
                {
                    fechaGeneracion = DateTime.Now,
                    rangoPrediccion = "7 días",
                    confianzaGeneral = "Alta",
                    produccionTotalSemana = Math.Round(prediccionSemanal.Sum(p => (decimal)((dynamic)p).produccionPredicha), 2),
                    promedioDiario = Math.Round(prediccionSemanal.Average(p => (decimal)((dynamic)p).produccionPredicha), 2),
                    predicciones = prediccionSemanal
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpGet("ranking-productoras")]
        public async Task<IActionResult> ObtenerRankingProductoras([FromQuery] int limite = 10)
        {
            try
            {
                var optimizaciones = await _servicioAnalisis.ObtenerOptimizacionesAsync();

                var ranking = optimizaciones
                    .OrderByDescending(o => o.ProduccionActual)
                    .Take(limite)
                    .Select((o, index) => new
                    {
                        posicion = index + 1,
                        idGanado = o.IdGanado,
                        nombreGanado = o.NombreGanado ?? $"Vaca #{o.IdGanado}",
                        produccionActual = Math.Round(o.ProduccionActual, 2),
                        produccionOptima = Math.Round(o.ProduccionOptima, 2),
                        eficiencia = o.ProduccionOptima > 0 ? Math.Round((o.ProduccionActual / o.ProduccionOptima) * 100, 1) : 0,
                        categoria = DeterminarCategoriaProductora(o.ProduccionActual),
                        potencialMejora = Math.Round(o.PotencialMejora, 2),
                        insignia = ObtenerInsignia(index + 1, o.ProduccionActual)
                    })
                    .ToList();

                return Ok(new
                {
                    totalVacas = optimizaciones.Count,
                    fechaRanking = DateTime.Now,
                    criterio = "Producción diaria actual",
                    promedioTop10 = ranking.Take(10).Average(r => (decimal)r.produccionActual),
                    promedioGeneral = optimizaciones.Average(o => o.ProduccionActual),
                    ranking = ranking
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        // Métodos auxiliares privados
        private string DeterminarEstadoGeneral(MetricasProduccionDto metricas)
        {
            var promedio = metricas.ProduccionPromedioPorVaca;

            if (promedio >= 25) return "Excelente";
            if (promedio >= 20) return "Bueno";
            if (promedio >= 15) return "Regular";
            return "Bajo";
        }

        private string DeterminarTendenciaReciente(List<TendenciaProduccionDto> tendencias)
        {
            if (tendencias.Count < 3) return "Insuficientes datos";

            var ultimos3 = tendencias.TakeLast(3).ToList();
            var primeros3 = tendencias.Take(3).ToList();

            var promedioReciente = ultimos3.Average(t => t.ProduccionReal);
            var promedioAnterior = primeros3.Average(t => t.ProduccionReal);

            var diferencia = promedioReciente - promedioAnterior;

            if (diferencia > promedioAnterior * 0.05) return "Creciente";
            if (diferencia < -promedioAnterior * 0.05) return "Decreciente";
            return "Estable";
        }

        private float CalcularVariacionPorcentual(List<TendenciaProduccionDto> tendencias)
        {
            if (tendencias.Count < 2) return 0;

            var primera = tendencias.First().ProduccionReal;
            var ultima = tendencias.Last().ProduccionReal;

            if (primera == 0) return 0;

            return ((ultima - primera) / primera) * 100;
        }

        private List<string> GenerarAlertasProduccion(MetricasProduccionDto metricas, List<TendenciaProduccionDto> tendencias)
        {
            var alertas = new List<string>();

            if (metricas.ProduccionPromedioPorVaca < 10)
            {
                alertas.Add("Producción promedio por vaca muy baja - Revisar alimentación");
            }

            if (metricas.PorcentajeCalidadAlta < 20)
            {
                alertas.Add("Bajo porcentaje de vacas con calidad alta - Implementar mejoras");
            }

            var tendenciaReciente = DeterminarTendenciaReciente(tendencias);
            if (tendenciaReciente == "Decreciente")
            {
                alertas.Add("Tendencia decreciente en producción - Requiere atención");
            }

            if (tendencias.Any() && tendencias.Average(t => t.DiferenciaAbsoluta) > 5)
            {
                alertas.Add("Alta variabilidad en predicciones - Revisar factores externos");
            }

            return alertas;
        }

        private float ObtenerFactorEstacional(DateTime fecha)
        {
            // Factor estacional simplificado basado en el mes
            int mes = fecha.Month;

            // Primavera (marzo-mayo): Mayor producción
            if (mes >= 3 && mes <= 5) return 1.1f;

            // Verano (junio-agosto): Producción moderada (estrés calórico)
            if (mes >= 6 && mes <= 8) return 0.95f;

            // Otoño (septiembre-noviembre): Buena producción
            if (mes >= 9 && mes <= 11) return 1.05f;

            // Invierno (diciembre-febrero): Producción variable
            return 1.0f;
        }

        private float ObtenerFactorTendencia(List<TendenciaProduccionDto> tendencias)
        {
            if (tendencias.Count < 7) return 1.0f;

            var ultimos7 = tendencias.TakeLast(7).ToList();
            var promedio = ultimos7.Average(t => t.ProduccionReal);
            var tendencia = DeterminarTendenciaReciente(tendencias);

            return tendencia switch
            {
                "Creciente" => 1.05f,
                "Decreciente" => 0.95f,
                _ => 1.0f
            };
        }

        private string CalcularConfianza(int diasAdelante)
        {
            return diasAdelante switch
            {
                1 => "Muy Alta",
                2 or 3 => "Alta",
                4 or 5 => "Media",
                _ => "Baja"
            };
        }

        private string DeterminarCategoriaProductora(float produccion)
        {
            if (produccion >= 30) return "Elite";
            if (produccion >= 25) return "Alta";
            if (produccion >= 20) return "Media";
            if (produccion >= 15) return "Básica";
            return "Baja";
        }

        private string ObtenerInsignia(int posicion, float produccion)
        {
            return posicion switch
            {
                1 => "🥇 Campeona",
                2 => "🥈 Subcampeona",
                3 => "🥉 Tercer lugar",
                _ when produccion >= 30 => "⭐ Elite",
                _ when produccion >= 25 => "🌟 Destacada",
                _ => "📈 Productora"
            };
        }
    }
}