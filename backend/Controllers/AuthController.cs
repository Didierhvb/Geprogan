using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GeproganAPP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GeproganAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GeproGanContext _context;
        private readonly IConfiguration _config;

        public AuthController(GeproGanContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public record LoginRequest(string Email, string Password);

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { message = "Email y contraseña son requeridos" });

            var user = _context.Usuarios.FirstOrDefault(u => u.EmailUr == req.Email);
            if (user == null)
                return Unauthorized(new { message = "Credenciales inválidas" });

            // Nota: Las contraseñas deberían estar hasheadas. Aquí se compara plano según el modelo actual.
            if (user.Contrasena != req.Password)
                return Unauthorized(new { message = "Credenciales inválidas" });

            var token = GenerateJwtToken(user.Idusuario, user.EmailUr, user.IdrolUr);
            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Idusuario,
                    nombre = user.NombreUr,
                    apellido = user.ApellidoUr,
                    email = user.EmailUr,
                    rolId = user.IdrolUr
                }
            });
        }

        private string GenerateJwtToken(int userId, string email, int? rolId)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("roleId", rolId?.ToString() ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSection["ExpiresMinutes"] ?? "120")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

