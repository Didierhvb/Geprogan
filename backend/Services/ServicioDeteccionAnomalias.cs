using Microsoft.ML;
using Microsoft.ML.Data;
using GeproganAPP.DTOs;
using GeproganAPP.Data;
using GeproganAPP.Models;
using Microsoft.EntityFrameworkCore;

namespace GeproganAPP.Services
{
    public interface IServicioDeteccionAnomalias
    {
        Task<bool> EntrenarModeloAsync();
        Task<AnalisisSaludAnimalDto> AnalizarSaludAnimalAsync(int idGanado);
        Task<MetricasSaludDto> ObtenerMetricasGeneralesAsync();
        Task<List<AnalisisSaludAnimalDto>> DetectarAnomaliasMasivasAsync();
    }

    public class ServicioDeteccionAnomalias : IServicioDeteccionAnomalias
    {
        private readonly GeproGanContext _context;
        private readonly MLContext _mlContext;
        private ITransformer? _modelo;
        private readonly string _rutaModelo = "modelo_anomalias_salud.zip";

        public ServicioDeteccionAnomalias(GeproGanContext context)
        {
            _context = context;
            _mlContext = new MLContext(seed: 0);
            CargarModeloExistente();
        }

        private void CargarModeloExistente()
        {
            if (File.Exists(_rutaModelo))
            {
                _modelo = _mlContext.Model.Load(_rutaModelo, out var modeloSchema);
            }
        }

        public async Task<bool> EntrenarModeloAsync()
        {
            try
            {
                var datosEntrenamiento = await ObtenerDatosEntrenamientoAsync();

                if (!datosEntrenamiento.Any())
                {
                    throw new InvalidOperationException("No hay suficientes datos para entrenar el modelo");
                }

                var dataView = _mlContext.Data.LoadFromEnumerable(datosEntrenamiento);

                var pipeline = _mlContext.Transforms.Concatenate("Features",
                    nameof(DatosSaludAnimal.PesoActual),
                    nameof(DatosSaludAnimal.PesoAnterior),
                    nameof(DatosSaludAnimal.CondicionCorporal),
                    nameof(DatosSaludAnimal.DiasDesdeUltimoPesaje),
                    nameof(DatosSaludAnimal.ProduccionPromedio),
                    nameof(DatosSaludAnimal.CantidadTratamientos),
                    nameof(DatosSaludAnimal.EdadEnDias))
                .Append(_mlContext.AnomalyDetection.Trainers.RandomizedPca(
                    featureColumnName: "Features",
                    rank: 5));

                _modelo = pipeline.Fit(dataView);
                _mlContext.Model.Save(_modelo, dataView.Schema, _rutaModelo);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error entrenando modelo: {ex.Message}");
                return false;
            }
        }

        private async Task<List<DatosSaludAnimal>> ObtenerDatosEntrenamientoAsync()
        {
            var query = from ganado in _context.Ganados
                        join pesaje in _context.Pesajes on ganado.Idganado equals pesaje.IdganadoPj into pesajes
                        from ultimoPesaje in pesajes.OrderByDescending(p => p.FechaPj).Take(1).DefaultIfEmpty()
                        join pesajeAnterior in _context.Pesajes on ganado.Idganado equals pesajeAnterior.IdganadoPj into pesajesAnteriores
                        from penultimoPesaje in pesajesAnteriores.OrderByDescending(p => p.FechaPj).Skip(1).Take(1).DefaultIfEmpty()
                        join produccion in _context.Produccions on ganado.Idganado equals produccion.IdganadoPr into producciones
                        join sanidad in _context.Sanidads on ganado.Idganado equals sanidad.IdganadoSn into sanidades
                        where ultimoPesaje != null
                        select new
                        {
                            ganado.Idganado,
                            PesoActual = ultimoPesaje.PesoPj ?? 0,
                            PesoAnterior = penultimoPesaje != null ? penultimoPesaje.PesoPj ?? 0 : ultimoPesaje.PesoPj ?? 0,
                            CondicionCorporal = ultimoPesaje.CondicionCorporal ?? 3,
                            UltimoPesaje = ultimoPesaje.FechaPj,
                            PenultimoPesaje = penultimoPesaje != null ? penultimoPesaje.FechaPj : ultimoPesaje.FechaPj,
                            ganado.FechaNacimiento,
                            Producciones = producciones.ToList(),
                            CantidadTratamientos = sanidades.Count()
                        };

            var datos = await query.ToListAsync();

            return datos.Select(d => new DatosSaludAnimal
            {
                IdGanado = d.Idganado,
                PesoActual = d.PesoActual,
                PesoAnterior = d.PesoAnterior,
                CondicionCorporal = d.CondicionCorporal,
                DiasDesdeUltimoPesaje = d.UltimoPesaje.HasValue && d.PenultimoPesaje.HasValue
                    ? (float)(d.UltimoPesaje.Value.ToDateTime(TimeOnly.MinValue) - d.PenultimoPesaje.Value.ToDateTime(TimeOnly.MinValue)).TotalDays
                    : 30,
                ProduccionPromedio = d.Producciones.Any() ? (float)d.Producciones.Average(p => (double)p.CantidadPr) : 0,
                CantidadTratamientos = d.CantidadTratamientos,
                EdadEnDias = (float)(DateTime.Now - d.FechaNacimiento.ToDateTime(TimeOnly.MinValue)).TotalDays
            }).ToList();
        }

        public async Task<AnalisisSaludAnimalDto> AnalizarSaludAnimalAsync(int idGanado)
        {
            if (_modelo == null)
            {
                await EntrenarModeloAsync();
            }

            var datosAnimal = await ObtenerDatosAnimalAsync(idGanado);
            if (datosAnimal == null)
            {
                throw new ArgumentException($"No se encontraron datos para el animal con ID {idGanado}");
            }

            var prediccion = PredecirAnomalia(datosAnimal);
            var ganado = await _context.Ganados.FindAsync(idGanado);

            var analisis = new AnalisisSaludAnimalDto
            {
                IdGanado = idGanado,
                NombreGanado = ganado?.NombreGanado,
                EsAnomalia = prediccion.EsAnomalia,
                PuntuacionAnomalia = prediccion.Puntuacion,
                FechaAnalisis = DateTime.Now,
                PesoActual = datosAnimal.PesoActual,
                CondicionCorporal = datosAnimal.CondicionCorporal,
                ProduccionPromedio = datosAnimal.ProduccionPromedio
            };

            // Determinar nivel de riesgo y alertas
            DeterminarRiesgoYAlertas(analisis, datosAnimal);

            return analisis;
        }

        private void DeterminarRiesgoYAlertas(AnalisisSaludAnimalDto analisis, DatosSaludAnimal datos)
        {
            if (analisis.PuntuacionAnomalia > 0.8f)
            {
                analisis.NivelRiesgo = "Alto";
                analisis.AlertasSalud.Add("Anomalía severa detectada - Requiere atención veterinaria inmediata");
            }
            else if (analisis.PuntuacionAnomalia > 0.6f)
            {
                analisis.NivelRiesgo = "Medio";
                analisis.AlertasSalud.Add("Anomalía moderada - Monitoreo cercano recomendado");
            }

            // Alertas específicas
            if (datos.PesoActual < datos.PesoAnterior * 0.9f)
            {
                analisis.AlertasSalud.Add("Pérdida significativa de peso detectada");
            }

            if (datos.CondicionCorporal <= 2)
            {
                analisis.AlertasSalud.Add("Condición corporal baja");
            }

            if (datos.CantidadTratamientos > 3)
            {
                analisis.AlertasSalud.Add("Múltiples tratamientos recientes");
            }

            if (datos.ProduccionPromedio < 10 && datos.ProduccionPromedio > 0)
            {
                analisis.AlertasSalud.Add("Producción por debajo del promedio");
            }
        }

        private async Task<DatosSaludAnimal?> ObtenerDatosAnimalAsync(int idGanado)
        {
            var datos = await (from ganado in _context.Ganados
                               where ganado.Idganado == idGanado
                               join pesaje in _context.Pesajes on ganado.Idganado equals pesaje.IdganadoPj into pesajes
                               from ultimoPesaje in pesajes.OrderByDescending(p => p.FechaPj).Take(1).DefaultIfEmpty()
                               join pesajeAnterior in _context.Pesajes on ganado.Idganado equals pesajeAnterior.IdganadoPj into pesajesAnteriores
                               from penultimoPesaje in pesajesAnteriores.OrderByDescending(p => p.FechaPj).Skip(1).Take(1).DefaultIfEmpty()
                               join produccion in _context.Produccions on ganado.Idganado equals produccion.IdganadoPr into producciones
                               join sanidad in _context.Sanidads on ganado.Idganado equals sanidad.IdganadoSn into sanidades
                               select new
                               {
                                   ganado.Idganado,
                                   PesoActual = ultimoPesaje != null ? ultimoPesaje.PesoPj ?? 0 : 0,
                                   PesoAnterior = penultimoPesaje != null ? penultimoPesaje.PesoPj ?? 0 : ultimoPesaje != null ? ultimoPesaje.PesoPj ?? 0 : 0,
                                   CondicionCorporal = ultimoPesaje != null ? ultimoPesaje.CondicionCorporal ?? 3 : 3,
                                   UltimoPesaje = ultimoPesaje != null ? ultimoPesaje.FechaPj : null,
                                   PenultimoPesaje = penultimoPesaje != null ? penultimoPesaje.FechaPj : null,
                                   ganado.FechaNacimiento,
                                   Producciones = producciones.ToList(),
                                   CantidadTratamientos = sanidades.Count()
                               }).FirstOrDefaultAsync();

            if (datos == null) return null;

            return new DatosSaludAnimal
            {
                IdGanado = datos.Idganado,
                PesoActual = datos.PesoActual,
                PesoAnterior = datos.PesoAnterior,
                CondicionCorporal = datos.CondicionCorporal,
                DiasDesdeUltimoPesaje = datos.UltimoPesaje.HasValue && datos.PenultimoPesaje.HasValue
                    ? (float)(datos.UltimoPesaje.Value.ToDateTime(TimeOnly.MinValue) - datos.PenultimoPesaje.Value.ToDateTime(TimeOnly.MinValue)).TotalDays
                    : 30,
                ProduccionPromedio = datos.Producciones.Any() ? (float)datos.Producciones.Average(p => (double)p.CantidadPr) : 0,
                CantidadTratamientos = datos.CantidadTratamientos,
                EdadEnDias = (float)(DateTime.Now - datos.FechaNacimiento.ToDateTime(TimeOnly.MinValue)).TotalDays
            };
        }

        private PrediccionAnomalíaSalud PredecirAnomalia(DatosSaludAnimal datos)
        {
            if (_modelo == null)
                throw new InvalidOperationException("El modelo no está entrenado");

            var predictor = _mlContext.Model.CreatePredictionEngine<DatosSaludAnimal, PrediccionAnomalíaSalud>(_modelo);
            return predictor.Predict(datos);
        }

        public async Task<List<AnalisisSaludAnimalDto>> DetectarAnomaliasMasivasAsync()
        {
            var animales = await _context.Ganados.Select(g => g.Idganado).ToListAsync();
            var resultados = new List<AnalisisSaludAnimalDto>();

            foreach (var idAnimal in animales)
            {
                try
                {
                    var analisis = await AnalizarSaludAnimalAsync(idAnimal);
                    if (analisis.EsAnomalia)
                    {
                        resultados.Add(analisis);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error analizando animal {idAnimal}: {ex.Message}");
                }
            }

            return resultados.OrderByDescending(r => r.PuntuacionAnomalia).ToList();
        }

        public async Task<MetricasSaludDto> ObtenerMetricasGeneralesAsync()
        {
            var anomalias = await DetectarAnomaliasMasivasAsync();
            var totalAnimales = await _context.Ganados.CountAsync();

            return new MetricasSaludDto
            {
                TotalAnimales = totalAnimales,
                AnimalesConAnomalias = anomalias.Count,
                AnimalesRiesgoAlto = anomalias.Count(a => a.NivelRiesgo == "Alto"),
                AnimalesRiesgoMedio = anomalias.Count(a => a.NivelRiesgo == "Medio"),
                PorcentajeAnomalias = totalAnimales > 0 ? (float)anomalias.Count / totalAnimales * 100 : 0,
                DetalleAnomalias = anomalias,
                FechaAnalisis = DateTime.Now
            };
        }
    }
}