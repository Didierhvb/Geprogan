using GeproganAPP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeproganAPP.Controllers
{
    [ApiController]
    [Route("api/usuarios/select")]
    [Authorize]
    public class UsuariosSelectController : ControllerBase
    {
        private readonly GeproGanContext _context;
        public UsuariosSelectController(GeproGanContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var usuarios = _context.Usuarios
                .Select(u => new {
                    id = u.Idusuario,
                    nombre = u.NombreUr,
                    apellido = u.ApellidoUr,
                    email = u.EmailUr
                })
                .ToList();
            return Ok(usuarios);
        }
    }
}
