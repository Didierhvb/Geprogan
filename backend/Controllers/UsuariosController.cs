using Microsoft.AspNetCore.Mvc;
using GeproganAPP.Data;

namespace GeproganAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly GeproGanContext _context;

        public UsuariosController(GeproGanContext context)
        {
            _context = context;
        }

        // GET: api/usuarios
        [HttpGet]
        public IActionResult GetUsuarios()
        {
            var usuarios = _context.Usuarios.ToList();
            return Ok(usuarios);
        }

        // GET: api/usuarios/5
        [HttpGet("{id}")]
        public IActionResult GetUsuario(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario == null)
                return NotFound();

            return Ok(usuario);        }

        public record UsuarioCreateDto(
            int? IdrolUr,
            string TipoIdentificacion,
            string NombreUr,
            string ApellidoUr,
            string EmailUr,
            string TelefonoUr,
            string Contrasena,
            string? UrlImageUr
        );

        [HttpPost]
        public IActionResult Create([FromBody] UsuarioCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.EmailUr) || string.IsNullOrWhiteSpace(dto.Contrasena))
                return BadRequest(new { message = "Email y contraseña son requeridos" });
            if (_context.Usuarios.Any(u => u.EmailUr == dto.EmailUr))
                return Conflict(new { message = "Email ya registrado" });

            var u = new GeproganAPP.Models.Usuario
            {
                IdrolUr = dto.IdrolUr,
                TipoIdentificacion = dto.TipoIdentificacion,
                NombreUr = dto.NombreUr,
                ApellidoUr = dto.ApellidoUr,
                EmailUr = dto.EmailUr,
                TelefonoUr = dto.TelefonoUr,
                Contrasena = dto.Contrasena,
                UrlImageUr = dto.UrlImageUr ?? string.Empty
            };

            _context.Usuarios.Add(u);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetUsuario), new { id = u.Idusuario }, u);
        }

        public record UsuarioUpdateDto(
            int? IdrolUr,
            string TipoIdentificacion,
            string NombreUr,
            string ApellidoUr,
            string EmailUr,
            string TelefonoUr,
            string? Contrasena,
            string? UrlImageUr
        );

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UsuarioUpdateDto dto)
        {
            var u = _context.Usuarios.Find(id);
            if (u == null) return NotFound(new { message = "Usuario no encontrado" });
            if (string.IsNullOrWhiteSpace(dto.EmailUr))
                return BadRequest(new { message = "Email es requerido" });
            if (_context.Usuarios.Any(x => x.EmailUr == dto.EmailUr && x.Idusuario != id))
                return Conflict(new { message = "Email ya registrado" });

            u.IdrolUr = dto.IdrolUr;
            u.TipoIdentificacion = dto.TipoIdentificacion;
            u.NombreUr = dto.NombreUr;
            u.ApellidoUr = dto.ApellidoUr;
            u.EmailUr = dto.EmailUr;
            u.TelefonoUr = dto.TelefonoUr;
            if (!string.IsNullOrWhiteSpace(dto.Contrasena))
                u.Contrasena = dto.Contrasena!;
            u.UrlImageUr = dto.UrlImageUr ?? u.UrlImageUr;

            _context.SaveChanges();
            return Ok(u);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var u = _context.Usuarios.Find(id);
            if (u == null) return NotFound(new { message = "Usuario no encontrado" });
            _context.Usuarios.Remove(u);
            _context.SaveChanges();
            return NoContent();
        }
        }
}


