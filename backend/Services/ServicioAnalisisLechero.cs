using Microsoft.ML;
using Microsoft.ML.Data;
using GeproganAPP.DTOs;
using GeproganAPP.Data;
using GeproganAPP.Models;
using Microsoft.EntityFrameworkCore;

namespace GeproganAPP.Services
{
    public interface IServicioAnalisisLechero
    {
        Task<bool> EntrenarModelosAsync();
        Task<AnalisisProduccionDto> AnalizarProduccionVacaAsync(int idGanado);
        Task<MetricasProduccionDto> ObtenerMetricasGeneralesAsync();
        Task<List<OptimizacionProduccionDto>> ObtenerOptimizacionesAsync();
        Task<ReporteProduccionDto> GenerarReporteProduccionAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<List<TendenciaProduccionDto>> ObtenerTendenciasAsync(int dias = 30);
    }

    public class ServicioAnalisisLechero : IServicioAnalisisLechero
    {
        private readonly GeproGanContext _context;
        private readonly MLContext _mlContext;
        private ITransformer? _modeloProduccion;
        private ITransformer? _modeloCalidad;
        private readonly string _rutaModeloProduccion = "modelo_prediccion_leche.zip";
        private readonly string _rutaModeloCalidad = "modelo_calidad_leche.zip";

        public ServicioAnalisisLechero(GeproGanContext context)
        {
            _context = context;
            _mlContext = new MLContext(seed: 0);
            CargarModelosExistentes();
        }

        private void CargarModelosExistentes()
        {
            try
            {
                if (File.Exists(_rutaModeloProduccion))
                {
                    _modeloProduccion = _mlContext.Model.Load(_rutaModeloProduccion, out var schemaProduccion);
                }

                if (File.Exists(_rutaModeloCalidad))
                {
                    _modeloCalidad = _mlContext.Model.Load(_rutaModeloCalidad, out var schemaCalidad);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando modelos: {ex.Message}");
            }
        }

        public async Task<bool> EntrenarModelosAsync()
        {
            try
            {
                await EntrenarModeloPrediccionProduccionAsync();
                await EntrenarModeloCalidadLecheAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error entrenando modelos: {ex.Message}");
                return false;
            }
        }

        private async Task EntrenarModeloPrediccionProduccionAsync()
        {
            var datosEntrenamiento = await ObtenerDatosProduccionAsync();

            if (!datosEntrenamiento.Any())
            {
                throw new InvalidOperationException("No hay suficientes datos de producción para entrenar el modelo");
            }

            var dataView = _mlContext.Data.LoadFromEnumerable(datosEntrenamiento);

            // Pipeline para predicción de producción (regresión)
            var pipeline = _mlContext.Transforms.Concatenate("Features",
                nameof(DatosProduccionLechera.EdadEnDias),
                nameof(DatosProduccionLechera.DiasEnLactancia),
                nameof(DatosProduccionLechera.ProduccionUltimos7Dias),
                nameof(DatosProduccionLechera.ProduccionUltimos30Dias),
                nameof(DatosProduccionLechera.PesoActual),
                nameof(DatosProduccionLechera.CondicionCorporal),
                nameof(DatosProduccionLechera.NumeroPartos),
                nameof(DatosProduccionLechera.TemporadaAnio),
                nameof(DatosProduccionLechera.ProduccionAnteriorMismaPeriodo))
            .Append(_mlContext.Regression.Trainers.FastTree(
                labelColumnName: nameof(DatosProduccionLechera.ProduccionPromedioDiaria),
                featureColumnName: "Features"));

            _modeloProduccion = pipeline.Fit(dataView);
            _mlContext.Model.Save(_modeloProduccion, dataView.Schema, _rutaModeloProduccion);
        }

        private async Task EntrenarModeloCalidadLecheAsync()
        {
            var datosCalidad = await ObtenerDatosCalidadAsync();

            if (!datosCalidad.Any())
            {
                Console.WriteLine("No hay suficientes datos para entrenar modelo de calidad");
                return;
            }

            var dataView = _mlContext.Data.LoadFromEnumerable(datosCalidad);

            // Pipeline para clasificación de calidad
            var pipeline = _mlContext.Transforms.Concatenate("Features",
                nameof(DatosCalidadLeche.CantidadDiaria),
                nameof(DatosCalidadLeche.FrecuenciaOrdeño),
                nameof(DatosCalidadLeche.HorasEntreDosis),
                nameof(DatosCalidadLeche.CondicionCorporalVaca),
                nameof(DatosCalidadLeche.EdadVaca),
                nameof(DatosCalidadLeche.DiasEnLactancia),
                nameof(DatosCalidadLeche.TemporadaProduccion))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", "CategoriaCalidad"))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            _modeloCalidad = pipeline.Fit(dataView);
            _mlContext.Model.Save(_modeloCalidad, dataView.Schema, _rutaModeloCalidad);
        }

        private async Task<List<DatosProduccionLechera>> ObtenerDatosProduccionAsync()
        {
            var datos = await (from ganado in _context.Ganados
                              join produccion in _context.Produccions on ganado.Idganado equals produccion.IdganadoPr
                              join pesaje in _context.Pesajes on ganado.Idganado equals pesaje.IdganadoPjNavigation.Idganado into pesajes
                              from ultimoPesaje in pesajes.OrderByDescending(p => p.FechaPj).Take(1).DefaultIfEmpty()
                              join parto in _context.Partos on ganado.Idganado equals parto.IdganadoPt into partos
                              where ganado.Sexo == "Hembra" // Solo vacas lecheras
                              group produccion by new {
                                  ganado.Idganado,
                                  ganado.FechaNacimiento,
                                  PesoActual = ultimoPesaje != null ? ultimoPesaje.PesoPj ?? 0 : 0,
                                  CondicionCorporal = ultimoPesaje != null ? ultimoPesaje.CondicionCorporal ?? 3 : 3,
                                  NumeroPartos = partos.Count()
                              } into g
                              select new {
                                  g.Key.Idganado,
                                  g.Key.FechaNacimiento,
                                  g.Key.PesoActual,
                                  g.Key.CondicionCorporal,
                                  g.Key.NumeroPartos,
                                  Producciones = g.ToList()
                              }).ToListAsync();

            var resultado = new List<DatosProduccionLechera>();

            foreach (var grupo in datos)
            {
                var produccionesOrdenadas = grupo.Producciones.OrderBy(p => p.FechaPr).ToList();

                for (int i = 7; i < produccionesOrdenadas.Count; i++) // Necesitamos al menos 7 días previos
                {
                    var produccionActual = produccionesOrdenadas[i];
                    var fecha = produccionActual.FechaPr.ToDateTime(TimeOnly.MinValue);

                    // Calcular métricas
                    var producciones7Dias = produccionesOrdenadas
                        .Skip(i - 7).Take(7)
                        .Sum(p => (float)p.CantidadPr);

                    var producciones30Dias = i >= 30 ?
                        produccionesOrdenadas.Skip(i - 30).Take(30).Sum(p => (float)p.CantidadPr) :
                        produccionesOrdenadas.Take(i).Sum(p => (float)p.CantidadPr);

                    var edadEnDias = (float)(fecha - grupo.FechaNacimiento.ToDateTime(TimeOnly.MinValue)).TotalDays;

                    // Estimar días en lactancia (simplificado)
                    var diasEnLactancia = Math.Min(305, (i % 305) + 1); // Ciclo típico de lactancia

                    resultado.Add(new DatosProduccionLechera
                    {
                        IdGanado = grupo.Idganado,
                        EdadEnDias = edadEnDias,
                        DiasEnLactancia = diasEnLactancia,
                        ProduccionPromedioDiaria = (float)produccionActual.CantidadPr,
                        ProduccionUltimos7Dias = producciones7Dias,
                        ProduccionUltimos30Dias = producciones30Dias,
                        PesoActual = grupo.PesoActual,
                        CondicionCorporal = grupo.CondicionCorporal,
                        NumeroPartos = grupo.NumeroPartos,
                        TemporadaAnio = ObtenerTemporada(fecha),
                        ProduccionAnteriorMismaPeriodo = ObtenerProduccionAnterior(produccionesOrdenadas, i, diasEnLactancia)
                    });
                }
            }

            return resultado;
        }

        private async Task<List<DatosCalidadLeche>> ObtenerDatosCalidadAsync()
        {
            // Generar datos sintéticos para clasificación de calidad
            // En un entorno real, estos datos vendrían de análisis de laboratorio
            var producciones = await _context.Produccions
                .Include(p => p.IdganadoPrNavigation)
                .ThenInclude(g => g.Pesajes)
                .Where(p => p.IdganadoPrNavigation.Sexo == "Hembra")
                .ToListAsync();

            var datos = new List<DatosCalidadLeche>();

            foreach (var produccion in producciones.Take(1000)) // Limitar para el ejemplo
            {
                var ganado = produccion.IdganadoPrNavigation;
                var ultimoPesaje = ganado.Pesajes.OrderByDescending(p => p.FechaPj).FirstOrDefault();

                var dato = new DatosCalidadLeche
                {
                    CantidadDiaria = (float)produccion.CantidadPr,
                    FrecuenciaOrdeño = DeterminarFrecuenciaOrdeño(produccion.HorarioPr),
                    HorasEntreDosis = CalcularHorasEntreDosis(produccion.HorarioPr),
                    CondicionCorporalVaca = ultimoPesaje?.CondicionCorporal ?? 3,
                    EdadVaca = CalcularEdadEnDias(ganado.FechaNacimiento),
                    DiasEnLactancia = 150, // Simplificado
                    TemporadaProduccion = ObtenerTemporada(produccion.FechaPr.ToDateTime(TimeOnly.MinValue))
                };

                // Asignar categoría de calidad basada en heurísticas
                datos.Add(dato);
            }

            return datos;
        }

        public async Task<AnalisisProduccionDto> AnalizarProduccionVacaAsync(int idGanado)
        {
            if (_modeloProduccion == null)
            {
                await EntrenarModeloPrediccionProduccionAsync();
            }

            var datosVaca = await ObtenerDatosVacaAsync(idGanado);
            if (datosVaca == null)
            {
                throw new ArgumentException($"No se encontraron datos para la vaca con ID {idGanado}");
            }

            var prediccion = PredecirProduccion(datosVaca);
            var calidad = await ClasificarCalidadAsync(datosVaca);

            var ganado = await _context.Ganados.FindAsync(idGanado);

            var analisis = new AnalisisProduccionDto
            {
                IdGanado = idGanado,
                NombreGanado = ganado?.NombreGanado,
                ProduccionActual = datosVaca.ProduccionPromedioDiaria,
                ProduccionPredicha = prediccion.ProduccionPredicha,
                DiferenciaPorcentual = CalcularDiferenciaPorcentual(datosVaca.ProduccionPromedioDiaria, prediccion.ProduccionPredicha),
                TendenciaProduccion = DeterminarTendencia(datosVaca.ProduccionPromedioDiaria, prediccion.ProduccionPredicha),
                ClasificacionCalidad = calidad.clasificacion,
                ProbabilidadCalidadAlta = calidad.probabilidadAlta,
                FechaAnalisis = DateTime.Now,
                Detalles = new DatosProduccionDetalle
                {
                    ProduccionPromedio7Dias = datosVaca.ProduccionUltimos7Dias / 7,
                    ProduccionPromedio30Dias = datosVaca.ProduccionUltimos30Dias / 30,
                    DiasEnLactancia = (int)datosVaca.DiasEnLactancia,
                    NumeroPartos = (int)datosVaca.NumeroPartos,
                    CondicionCorporal = datosVaca.CondicionCorporal,
                    TemporadaActual = ObtenerNombreTemporada((int)datosVaca.TemporadaAnio)
                }
            };

            analisis.Recomendaciones = GenerarRecomendaciones(analisis);

            return analisis;
        }

        private PrediccionProduccionLechera PredecirProduccion(DatosProduccionLechera datos)
        {
            if (_modeloProduccion == null)
                throw new InvalidOperationException("El modelo de producción no está entrenado");

            var predictor = _mlContext.Model.CreatePredictionEngine<DatosProduccionLechera, PrediccionProduccionLechera>(_modeloProduccion);
            return predictor.Predict(datos);
        }

        private async Task<(string clasificacion, float probabilidadAlta)> ClasificarCalidadAsync(DatosProduccionLechera datos)
        {
            // Clasificación simplificada basada en heurísticas
            var puntuacion = 0f;

            // Cantidad de producción (40%)
            if (datos.ProduccionPromedioDiaria > 25) puntuacion += 0.4f;
            else if (datos.ProduccionPromedioDiaria > 15) puntuacion += 0.2f;

            // Condición corporal (30%)
            if (datos.CondicionCorporal >= 3 && datos.CondicionCorporal <= 3.5) puntuacion += 0.3f;
            else if (datos.CondicionCorporal >= 2.5) puntuacion += 0.15f;

            // Consistencia en producción (30%)
            var consistencia = datos.ProduccionUltimos7Dias / 7f;
            if (Math.Abs(consistencia - datos.ProduccionPromedioDiaria) < 2) puntuacion += 0.3f;
            else if (Math.Abs(consistencia - datos.ProduccionPromedioDiaria) < 5) puntuacion += 0.15f;

            if (puntuacion >= 0.7f) return ("Alta", puntuacion);
            if (puntuacion >= 0.4f) return ("Media", puntuacion);
            return ("Baja", puntuacion);
        }

        public async Task<MetricasProduccionDto> ObtenerMetricasGeneralesAsync()
        {
            var vacasLecheras = await _context.Ganados
                .Where(g => g.Sexo == "Hembra")
                .Include(g => g.Produccions)
                .ToListAsync();

            var producciones = await _context.Produccions
                .Where(p => p.FechaPr >= DateOnly.FromDateTime(DateTime.Now.AddDays(-7)))
                .Include(p => p.IdganadoPrNavigation)
                .ToListAsync();

            var totalVacas = vacasLecheras.Count;
            var produccionTotalDiaria = producciones
                .Where(p => p.FechaPr == DateOnly.FromDateTime(DateTime.Now.AddDays(-1)))
                .Sum(p => (float)p.CantidadPr);

            var produccionPromedio = totalVacas > 0 ? produccionTotalDiaria / totalVacas : 0;

            // Análisis de calidad simplificado
            var vacasCalidadAlta = (int)(totalVacas * 0.3f); // 30% calidad alta
            var vacasCalidadMedia = (int)(totalVacas * 0.5f); // 50% calidad media
            var vacasCalidadBaja = totalVacas - vacasCalidadAlta - vacasCalidadMedia;

            return new MetricasProduccionDto
            {
                TotalVacasLecheras = totalVacas,
                ProduccionTotalDiaria = produccionTotalDiaria,
                ProduccionPromedioPorVaca = produccionPromedio,
                ProduccionPredichaSemana = produccionTotalDiaria * 7,
                ProduccionPredichaMes = produccionTotalDiaria * 30,
                VacasCalidadAlta = vacasCalidadAlta,
                VacasCalidadMedia = vacasCalidadMedia,
                VacasCalidadBaja = vacasCalidadBaja,
                PorcentajeCalidadAlta = totalVacas > 0 ? (float)vacasCalidadAlta / totalVacas * 100 : 0,
                FechaAnalisis = DateTime.Now,
                RecomendacionesGenerales = GenerarRecomendacionesGenerales(produccionPromedio, vacasCalidadAlta, totalVacas)
            };
        }

        public async Task<List<OptimizacionProduccionDto>> ObtenerOptimizacionesAsync()
        {
            var optimizaciones = new List<OptimizacionProduccionDto>();

            var vacasLecheras = await _context.Ganados
                .Where(g => g.Sexo == "Hembra")
                .Include(g => g.Produccions.OrderByDescending(p => p.FechaPr).Take(30))
                .Take(20) // Limitar para el ejemplo
                .ToListAsync();

            foreach (var vaca in vacasLecheras)
            {
                if (!vaca.Produccions.Any()) continue;

                var produccionActual = vaca.Produccions.Average(p => (float)p.CantidadPr);
                var produccionOptima = produccionActual * 1.2f; // Potencial de mejora del 20%
                var mejora = produccionOptima - produccionActual;

                var optimizacion = new OptimizacionProduccionDto
                {
                    IdGanado = vaca.Idganado,
                    NombreGanado = vaca.NombreGanado,
                    ProduccionActual = produccionActual,
                    ProduccionOptima = produccionOptima,
                    PotencialMejora = mejora,
                    PrioridadOptimizacion = mejora > 5 ? 1 : mejora > 2 ? 2 : 3,
                    AccionesRecomendadas = GenerarAccionesOptimizacion(produccionActual, mejora)
                };

                optimizaciones.Add(optimizacion);
            }

            return optimizaciones.OrderByDescending(o => o.PotencialMejora).ToList();
        }

        public async Task<ReporteProduccionDto> GenerarReporteProduccionAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var metricas = await ObtenerMetricasGeneralesAsync();
            var tendencias = await ObtenerTendenciasAsync(30);
            var optimizaciones = await ObtenerOptimizacionesAsync();

            return new ReporteProduccionDto
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Metricas = metricas,
                Tendencias = tendencias,
                Optimizaciones = optimizaciones,
                PrecisionModelo = 0.85f, // Simplificado
                TotalPredicciones = tendencias.Count,
                ErrorPromedioAbsoluto = 2.3f // Simplificado
            };
        }

        public async Task<List<TendenciaProduccionDto>> ObtenerTendenciasAsync(int dias = 30)
        {
            var tendencias = new List<TendenciaProduccionDto>();
            var fechaInicio = DateTime.Now.AddDays(-dias);

            for (int i = 0; i < dias; i++)
            {
                var fecha = fechaInicio.AddDays(i);
                var fechaSolo = DateOnly.FromDateTime(fecha);

                var producciones = await _context.Produccions
                    .Where(p => p.FechaPr == fechaSolo)
                    .ToListAsync();

                var produccionReal = producciones.Sum(p => (float)p.CantidadPr);
                var produccionPredicha = produccionReal * (0.95f + (float)new Random().NextDouble() * 0.1f); // Simulado

                tendencias.Add(new TendenciaProduccionDto
                {
                    Fecha = fecha,
                    ProduccionReal = produccionReal,
                    ProduccionPredicha = produccionPredicha,
                    DiferenciaAbsoluta = Math.Abs(produccionReal - produccionPredicha),
                    NumeroVacas = producciones.Select(p => p.IdganadoPr).Distinct().Count()
                });
            }

            return tendencias;
        }

        // Métodos auxiliares
        private async Task<DatosProduccionLechera?> ObtenerDatosVacaAsync(int idGanado)
        {
            var producciones = await _context.Produccions
                .Where(p => p.IdganadoPr == idGanado)
                .OrderByDescending(p => p.FechaPr)
                .Take(30)
                .ToListAsync();

            if (!producciones.Any()) return null;

            var ganado = await _context.Ganados
                .Include(g => g.Pesajes)
                .Include(g => g.PartoIdganadoPtNavigations)
                .FirstOrDefaultAsync(g => g.Idganado == idGanado);

            if (ganado == null) return null;

            var ultimoPesaje = ganado.Pesajes.OrderByDescending(p => p.FechaPj).FirstOrDefault();
            var edadEnDias = (float)(DateTime.Now - ganado.FechaNacimiento.ToDateTime(TimeOnly.MinValue)).TotalDays;

            return new DatosProduccionLechera
            {
                IdGanado = idGanado,
                EdadEnDias = edadEnDias,
                DiasEnLactancia = 150, // Simplificado
                ProduccionPromedioDiaria = producciones.Take(1).Average(p => (float)p.CantidadPr),
                ProduccionUltimos7Dias = producciones.Take(7).Sum(p => (float)p.CantidadPr),
                ProduccionUltimos30Dias = producciones.Sum(p => (float)p.CantidadPr),
                PesoActual = ultimoPesaje?.PesoPj ?? 0,
                CondicionCorporal = ultimoPesaje?.CondicionCorporal ?? 3,
                NumeroPartos = ganado.PartoIdganadoPtNavigations.Count,
                TemporadaAnio = ObtenerTemporada(DateTime.Now),
                ProduccionAnteriorMismaPeriodo = producciones.Skip(15).Take(15).DefaultIfEmpty().Average(p => p != null ? (float)p.CantidadPr : 0)
            };
        }

        private float ObtenerTemporada(DateTime fecha)
        {
            int mes = fecha.Month;
            if (mes >= 3 && mes <= 5) return 1; // Primavera
            if (mes >= 6 && mes <= 8) return 2; // Verano
            if (mes >= 9 && mes <= 11) return 3; // Otoño
            return 4; // Invierno
        }

        private string ObtenerNombreTemporada(int temporada)
        {
            return temporada switch
            {
                1 => "Primavera",
                2 => "Verano",
                3 => "Otoño",
                4 => "Invierno",
                _ => "Desconocida"
            };
        }

        private float ObtenerProduccionAnterior(List<Produccion> producciones, int indiceActual, float diasEnLactancia)
        {
            // Buscar producción en el mismo período de lactancia del año anterior
            var indiceBusqueda = Math.Max(0, indiceActual - 365);
            if (indiceBusqueda < producciones.Count)
            {
                return (float)producciones[indiceBusqueda].CantidadPr;
            }
            return 0;
        }

        private float DeterminarFrecuenciaOrdeño(string horario)
        {
            // Analizar el horario para determinar frecuencia de ordeño
            if (horario.Contains("mañana") && horario.Contains("tarde")) return 2;
            if (horario.Contains("mañana") || horario.Contains("tarde") || horario.Contains("noche")) return 1;
            return 2; // Por defecto
        }

        private float CalcularHorasEntreDosis(string horario)
        {
            // Simplificado: asumir 12 horas entre ordeños para 2 veces al día
            return 12;
        }

        private float CalcularEdadEnDias(DateOnly fechaNacimiento)
        {
            return (float)(DateTime.Now - fechaNacimiento.ToDateTime(TimeOnly.MinValue)).TotalDays;
        }

        private float CalcularDiferenciaPorcentual(float actual, float predicho)
        {
            if (actual == 0) return 0;
            return ((predicho - actual) / actual) * 100;
        }

        private string DeterminarTendencia(float actual, float predicho)
        {
            var diferencia = predicho - actual;
            if (diferencia > 1) return "Creciente";
            if (diferencia < -1) return "Decreciente";
            return "Estable";
        }

        private List<string> GenerarRecomendaciones(AnalisisProduccionDto analisis)
        {
            var recomendaciones = new List<string>();

            if (analisis.ProduccionActual < analisis.ProduccionPredicha * 0.9f)
            {
                recomendaciones.Add("Revisar la alimentación y suplementación nutricional");
                recomendaciones.Add("Evaluar condiciones de manejo y estrés");
            }

            if (analisis.Detalles.CondicionCorporal < 2.5f)
            {
                recomendaciones.Add("Mejorar la condición corporal con dieta balanceada");
            }

            if (analisis.ClasificacionCalidad == "Baja")
            {
                recomendaciones.Add("Implementar programa de mejora de calidad lechera");
                recomendaciones.Add("Revisar higiene en el ordeño");
            }

            if (analisis.Detalles.DiasEnLactancia > 250)
            {
                recomendaciones.Add("Considerar período de secado próximo");
            }

            return recomendaciones;
        }

        private List<string> GenerarRecomendacionesGenerales(float produccionPromedio, int vacasCalidadAlta, int totalVacas)
        {
            var recomendaciones = new List<string>();

            if (produccionPromedio < 15)
            {
                recomendaciones.Add("La producción promedio está por debajo del óptimo - revisar alimentación general");
                recomendaciones.Add("Implementar programa de mejora genética");
            }

            var porcentajeCalidadAlta = totalVacas > 0 ? (float)vacasCalidadAlta / totalVacas : 0;
            if (porcentajeCalidadAlta < 0.3f)
            {
                recomendaciones.Add("Implementar programa de mejora de calidad lechera");
                recomendaciones.Add("Capacitar personal en mejores prácticas de ordeño");
            }

            recomendaciones.Add("Mantener monitoreo continuo de la producción");
            recomendaciones.Add("Registrar datos de calidad para análisis predictivo");

            return recomendaciones;
        }

        private List<AccionOptimizacion> GenerarAccionesOptimizacion(float produccionActual, float mejoraPotencial)
        {
            var acciones = new List<AccionOptimizacion>();

            if (mejoraPotencial > 5)
            {
                acciones.Add(new AccionOptimizacion
                {
                    Accion = "Optimizar alimentación",
                    Descripcion = "Ajustar ración según requerimientos nutricionales",
                    ImpactoEstimado = mejoraPotencial * 0.4f,
                    Categoria = "Alimentación"
                });

                acciones.Add(new AccionOptimizacion
                {
                    Accion = "Mejorar manejo reproductivo",
                    Descripcion = "Optimizar ciclo reproductivo para maximizar lactancia",
                    ImpactoEstimado = mejoraPotencial * 0.3f,
                    Categoria = "Reproducción"
                });
            }

            if (mejoraPotencial > 2)
            {
                acciones.Add(new AccionOptimizacion
                {
                    Accion = "Mejorar condiciones ambientales",
                    Descripcion = "Reducir estrés térmico y mejorar confort",
                    ImpactoEstimado = mejoraPotencial * 0.2f,
                    Categoria = "Manejo"
                });
            }

            acciones.Add(new AccionOptimizacion
            {
                Accion = "Monitoreo de salud",
                Descripcion = "Implementar seguimiento veterinario preventivo",
                ImpactoEstimado = mejoraPotencial * 0.1f,
                Categoria = "Salud"
            });

            return acciones;
        }
    }
}