using GeproganAPP.Data;
using GeproganAPP.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<GeproGanContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GeproGanDb")));

// Registrar servicios de ML.NET
builder.Services.AddScoped<IServicioDeteccionAnomalias, ServicioDeteccionAnomalias>();
builder.Services.AddScoped<IServicioAnalisisLechero, ServicioAnalisisLechero>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (permitir front local)
const string CorsPolicyName = "AllowAllDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CorsPolicyName, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]!;
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Evitar 307 Redirect en preflight CORS desde HTTP (origen file://)
    // No forzar HTTPS en desarrollo para endpoints consumidos desde archivos locales
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicyName);

// Servir archivos estáticos del frontend
app.UseStaticFiles();
app.UseDefaultFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Fallback para SPA - redirigir rutas no encontradas a index.html
app.MapFallbackToFile("index.html");

app.Run();
