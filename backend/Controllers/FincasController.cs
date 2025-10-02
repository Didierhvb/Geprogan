using GeproganAPP.Data;
using GeproganAPP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GeproganAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FincasController : ControllerBase
    {
        private readonly GeproGanContext _context;
        public FincasController(GeproGanContext context) { _context = context; }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no identificado" });

            var fincas = await _context.Fincas
                .Include(f => f.PropietarioNavigation)
                .Where(f => f.Propietario == userId)
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
            decimal? Latitud,
            decimal? Longitud
        );

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FincaCreateDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no identificado" });

            if (string.IsNullOrWhiteSpace(dto.NombreFinca) || string.IsNullOrWhiteSpace(dto.Ubicacion))
            {
                return BadRequest(new { message = "NombreFinca y Ubicacion son requeridos" });
            }
            if (dto.Hectareas <= 0)
            {
                return BadRequest(new { message = "Hectareas debe ser mayor a 0" });
            }

            // La finca se crea automáticamente para el usuario autenticado
            var finca = new Finca
            {
                NombreFinca = dto.NombreFinca,
                Ubicacion = dto.Ubicacion,
                Hectareas = dto.Hectareas,
                Propietario = userId, // Siempre el usuario autenticado
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
            decimal? Latitud,
            decimal? Longitud
        );

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FincaUpdateDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no identificado" });

            var finca = await _context.Fincas.FindAsync(id);
            if (finca == null) return NotFound(new { message = "Finca no encontrada" });

            // Verificar que la finca pertenece al usuario autenticado
            if (finca.Propietario != userId)
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.NombreFinca) || string.IsNullOrWhiteSpace(dto.Ubicacion))
            {
                return BadRequest(new { message = "NombreFinca y Ubicacion son requeridos" });
            }
            if (dto.Hectareas <= 0)
            {
                return BadRequest(new { message = "Hectareas debe ser mayor a 0" });
            }

            finca.NombreFinca = dto.NombreFinca;
            finca.Ubicacion = dto.Ubicacion;
            finca.Hectareas = dto.Hectareas;
            // No permitir cambiar el propietario
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
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no identificado" });

            var finca = await _context.Fincas.FindAsync(id);
            if (finca == null) return NotFound(new { message = "Finca no encontrada" });

            // Verificar que la finca pertenece al usuario autenticado
            if (finca.Propietario != userId)
                return Forbid();

            _context.Fincas.Remove(finca);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
