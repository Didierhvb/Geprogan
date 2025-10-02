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
    public class GanadoController : ControllerBase
    {
        private readonly GeproGanContext _context;

        public GanadoController(GeproGanContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        public record GanadoCreateDto(
            int Idfinca,
            int IdtipoGanado,
            string FechaNacimiento, // yyyy-MM-dd
            string Sexo,
            string? MarcaGanado,
            string? Raza,
            string? Caracteristicas,
            string? NombreGanado,
            int? NumeroId,
            int? NumeroInventario,
            string? UrlImagen
        );

        [HttpGet]
        public IActionResult GetAll()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no identificado" });

            // Filtrar ganado solo de fincas que pertenecen al usuario autenticado
            var list = (from g in _context.Ganados
                        join f in _context.Fincas on g.Idfinca equals f.Idfinca
                        where f.Propietario == userId
                        select new
                        {
                            g.Idganado,
                            g.Idfinca,
                            g.IdtipoGanado,
                            g.FechaNacimiento,
                            g.Sexo,
                            g.MarcaGanado,
                            g.Raza,
                            g.Caracteristicas,
                            g.NombreGanado,
                            g.NumeroId,
                            g.NumeroInventario,
                            g.UrlImagen
                        }).ToList();

            return Ok(list);
        }

        [HttpPost]
        public IActionResult Create([FromBody] GanadoCreateDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no identificado" });

            if (!DateOnly.TryParse(dto.FechaNacimiento, out var fechaNac))
            {
                return BadRequest(new { message = "FechaNacimiento inválida, use formato yyyy-MM-dd" });
            }

            // Verificar que la finca pertenece al usuario autenticado
            var finca = _context.Fincas.Find(dto.Idfinca);
            if (finca == null)
                return BadRequest(new { message = "Finca no encontrada" });

            if (finca.Propietario != userId)
                return Forbid(); // 403 - El usuario no tiene permiso sobre esta finca

            var ganado = new Ganado
            {
                Idfinca = dto.Idfinca,
                IdtipoGanado = dto.IdtipoGanado,
                FechaNacimiento = fechaNac,
                Sexo = dto.Sexo,
                MarcaGanado = dto.MarcaGanado,
                Raza = dto.Raza,
                Caracteristicas = dto.Caracteristicas,
                NombreGanado = dto.NombreGanado,
                NumeroId = dto.NumeroId,
                NumeroInventario = dto.NumeroInventario,
                UrlImagen = dto.UrlImagen
            };

            _context.Ganados.Add(ganado);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = ganado.Idganado }, new
            {
                ganado.Idganado,
                ganado.Idfinca,
                ganado.IdtipoGanado,
                ganado.FechaNacimiento,
                ganado.Sexo,
                ganado.MarcaGanado,
                ganado.Raza,
                ganado.Caracteristicas,
                ganado.NombreGanado,
                ganado.NumeroId,
                ganado.NumeroInventario,
                ganado.UrlImagen
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no identificado" });

            var g = (from ganado in _context.Ganados
                     join f in _context.Fincas on ganado.Idfinca equals f.Idfinca
                     where ganado.Idganado == id && f.Propietario == userId
                     select new
                     {
                         ganado.Idganado,
                         ganado.Idfinca,
                         ganado.IdtipoGanado,
                         ganado.FechaNacimiento,
                         ganado.Sexo,
                         ganado.MarcaGanado,
                         ganado.Raza,
                         ganado.Caracteristicas,
                         ganado.NombreGanado,
                         ganado.NumeroId,
                         ganado.NumeroInventario,
                         ganado.UrlImagen
                     }).FirstOrDefault();

            if (g == null) return NotFound();

            return Ok(g);
        }

        public record GanadoUpdateDto(
            int Idfinca,
            int IdtipoGanado,
            string FechaNacimiento,
            string Sexo,
            string? MarcaGanado,
            string? Raza,
            string? Caracteristicas,
            string? NombreGanado,
            int? NumeroId,
            int? NumeroInventario,
            string? UrlImagen
        );

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] GanadoUpdateDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no identificado" });

            var ganado = _context.Ganados.Find(id);
            if (ganado == null) return NotFound(new { message = "Ganado no encontrado" });

            // Verificar que el ganado pertenece a una finca del usuario
            var fincaActual = _context.Fincas.Find(ganado.Idfinca);
            if (fincaActual == null || fincaActual.Propietario != userId)
                return Forbid();

            if (!DateOnly.TryParse(dto.FechaNacimiento, out var fechaNac))
            {
                return BadRequest(new { message = "FechaNacimiento inválida, use formato yyyy-MM-dd" });
            }

            // Verificar que la nueva finca también pertenece al usuario
            if (dto.Idfinca != ganado.Idfinca)
            {
                var newFinca = _context.Fincas.Find(dto.Idfinca);
                if (newFinca == null)
                    return BadRequest(new { message = "Finca no encontrada" });
                if (newFinca.Propietario != userId)
                    return Forbid();
            }

            ganado.Idfinca = dto.Idfinca;
            ganado.IdtipoGanado = dto.IdtipoGanado;
            ganado.FechaNacimiento = fechaNac;
            ganado.Sexo = dto.Sexo;
            ganado.MarcaGanado = dto.MarcaGanado;
            ganado.Raza = dto.Raza;
            ganado.Caracteristicas = dto.Caracteristicas;
            ganado.NombreGanado = dto.NombreGanado;
            ganado.NumeroId = dto.NumeroId;
            ganado.NumeroInventario = dto.NumeroInventario;
            ganado.UrlImagen = dto.UrlImagen;

            _context.SaveChanges();
            return Ok(new
            {
                ganado.Idganado,
                ganado.Idfinca,
                ganado.IdtipoGanado,
                ganado.FechaNacimiento,
                ganado.Sexo,
                ganado.MarcaGanado,
                ganado.Raza,
                ganado.Caracteristicas,
                ganado.NombreGanado,
                ganado.NumeroId,
                ganado.NumeroInventario,
                ganado.UrlImagen
            });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no identificado" });

            var ganado = _context.Ganados.Find(id);
            if (ganado == null) return NotFound(new { message = "Ganado no encontrado" });

            // Verificar que el ganado pertenece a una finca del usuario
            var finca = _context.Fincas.Find(ganado.Idfinca);
            if (finca == null || finca.Propietario != userId)
                return Forbid();

            _context.Ganados.Remove(ganado);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
