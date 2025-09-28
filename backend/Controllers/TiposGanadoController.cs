using GeproganAPP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeproganAPP.Models;

namespace GeproganAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TiposGanadoController : ControllerBase
    {
        private readonly GeproGanContext _context;
        public TiposGanadoController(GeproGanContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var tipos = await _context.TipoGanados
                .Select(t => new { id = t.IdtipoGanado, nombre = t.NombreTg })
                .ToListAsync();
            return Ok(tipos);
        }

        public record TipoGanadoCreateDto(string Nombre);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TipoGanadoCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { message = "Nombre es requerido" });
            var tg = new TipoGanado { NombreTg = dto.Nombre.Trim() };
            _context.TipoGanados.Add(tg);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = tg.IdtipoGanado }, new { id = tg.IdtipoGanado, nombre = tg.NombreTg });
        }

        public record TipoGanadoUpdateDto(string Nombre);

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TipoGanadoUpdateDto dto)
        {
            var tg = await _context.TipoGanados.FindAsync(id);
            if (tg == null) return NotFound(new { message = "Tipo de ganado no encontrado" });
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { message = "Nombre es requerido" });
            tg.NombreTg = dto.Nombre.Trim();
            await _context.SaveChangesAsync();
            return Ok(new { id = tg.IdtipoGanado, nombre = tg.NombreTg });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tg = await _context.TipoGanados.FindAsync(id);
            if (tg == null) return NotFound(new { message = "Tipo de ganado no encontrado" });
            _context.TipoGanados.Remove(tg);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
