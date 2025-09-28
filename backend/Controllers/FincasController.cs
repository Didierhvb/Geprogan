using GeproganAPP.Data;
using GeproganAPP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeproganAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FincasController : ControllerBase
    {
        private readonly GeproGanContext _context;
        public FincasController(GeproGanContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var fincas = await _context.Fincas
                .Include(f => f.PropietarioNavigation)
                .Select(f => new
                {
                    idfinca = f.Idfinca,
                    nombreFinca = f.NombreFinca,
                    ubicacion = f.Ubicacion,
                    latitud = f.Latitud,
                    longitud = f.Longitud,
                    hectareas = f.Hectareas,
                    propietario = f.Propietario,
                    propietarioNombre = (f.PropietarioNavigation.NombreUr + " " + f.PropietarioNavigation.ApellidoUr).Trim()
                })
                .ToListAsync();
            return Ok(fincas);
        }

        public record FincaCreateDto(
            string NombreFinca,
            string Ubicacion,
            decimal Hectareas,
            int Propietario,
            decimal? Latitud,
            decimal? Longitud
        );

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FincaCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NombreFinca) || string.IsNullOrWhiteSpace(dto.Ubicacion))
            {
                return BadRequest(new { message = "NombreFinca y Ubicacion son requeridos" });
            }
            if (dto.Hectareas <= 0)
            {
                return BadRequest(new { message = "Hectareas debe ser mayor a 0" });
            }
            if (dto.Propietario <= 0)
            {
                return BadRequest(new { message = "Propietario inválido" });
            }

            var finca = new Finca
            {
                NombreFinca = dto.NombreFinca,
                Ubicacion = dto.Ubicacion,
                Hectareas = dto.Hectareas,
                Propietario = dto.Propietario,
                Latitud = dto.Latitud,
                Longitud = dto.Longitud
            };

            _context.Fincas.Add(finca);
            await _context.SaveChangesAsync();

            var propietario = await _context.Usuarios.FindAsync(finca.Propietario);
            return CreatedAtAction(nameof(Get), new { id = finca.Idfinca }, new
            {
                idfinca = finca.Idfinca,
                nombreFinca = finca.NombreFinca,
                ubicacion = finca.Ubicacion,
                latitud = finca.Latitud,
                longitud = finca.Longitud,
                hectareas = finca.Hectareas,
                propietario = finca.Propietario,
                propietarioNombre = propietario != null ? ($"{propietario.NombreUr} {propietario.ApellidoUr}").Trim() : string.Empty
            });
        }

        public record FincaUpdateDto(
            string NombreFinca,
            string Ubicacion,
            decimal Hectareas,
            int Propietario,
            decimal? Latitud,
            decimal? Longitud
        );

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FincaUpdateDto dto)
        {
            var finca = await _context.Fincas.FindAsync(id);
            if (finca == null) return NotFound(new { message = "Finca no encontrada" });

            if (string.IsNullOrWhiteSpace(dto.NombreFinca) || string.IsNullOrWhiteSpace(dto.Ubicacion))
            {
                return BadRequest(new { message = "NombreFinca y Ubicacion son requeridos" });
            }
            if (dto.Hectareas <= 0)
            {
                return BadRequest(new { message = "Hectareas debe ser mayor a 0" });
            }
            if (dto.Propietario <= 0)
            {
                return BadRequest(new { message = "Propietario inválido" });
            }

            finca.NombreFinca = dto.NombreFinca;
            finca.Ubicacion = dto.Ubicacion;
            finca.Hectareas = dto.Hectareas;
            finca.Propietario = dto.Propietario;
            finca.Latitud = dto.Latitud;
            finca.Longitud = dto.Longitud;

            await _context.SaveChangesAsync();

            var propietario = await _context.Usuarios.FindAsync(finca.Propietario);
            return Ok(new
            {
                idfinca = finca.Idfinca,
                nombreFinca = finca.NombreFinca,
                ubicacion = finca.Ubicacion,
                latitud = finca.Latitud,
                longitud = finca.Longitud,
                hectareas = finca.Hectareas,
                propietario = finca.Propietario,
                propietarioNombre = propietario != null ? ($"{propietario.NombreUr} {propietario.ApellidoUr}").Trim() : string.Empty
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var finca = await _context.Fincas.FindAsync(id);
            if (finca == null) return NotFound(new { message = "Finca no encontrada" });

            _context.Fincas.Remove(finca);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
