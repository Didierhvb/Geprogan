using GeproganAPP.Data;
using GeproganAPP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var list = _context.Ganados.ToList();
            return Ok(list);
        }

        [HttpPost]
        public IActionResult Create([FromBody] GanadoCreateDto dto)
        {
            if (!DateOnly.TryParse(dto.FechaNacimiento, out var fechaNac))
            {
                return BadRequest(new { message = "FechaNacimiento inválida, use formato yyyy-MM-dd" });
            }

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
            return CreatedAtAction(nameof(GetById), new { id = ganado.Idganado }, ganado);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var g = _context.Ganados.Find(id);
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
            var ganado = _context.Ganados.Find(id);
            if (ganado == null) return NotFound(new { message = "Ganado no encontrado" });

            if (!DateOnly.TryParse(dto.FechaNacimiento, out var fechaNac))
            {
                return BadRequest(new { message = "FechaNacimiento inválida, use formato yyyy-MM-dd" });
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
            return Ok(ganado);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var ganado = _context.Ganados.Find(id);
            if (ganado == null) return NotFound(new { message = "Ganado no encontrado" });
            _context.Ganados.Remove(ganado);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
