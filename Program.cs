using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Config DataBase
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(8, 0, 25))));

// Config JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");;
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured.");


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidIssuer = jwtIssuer
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();


app.MapPost("/login", async (string email, string password, ApplicationDbContext context) =>
{
    var admin = await context.Administrators.FirstOrDefaultAsync(a => a.Email == email && a.Password == password);
    if (admin is null)
        return Results.Unauthorized();

    var token = GenerateJwtToken(admin, jwtKey, jwtIssuer);
    return Results.Ok(new { Token = token });
});

// Function to generate token JWT
static string GenerateJwtToken(Administrator admin, string key, string issuer)
{
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, admin.Name ?? "NoName"),
        new Claim(JwtRegisteredClaimNames.Email, admin.Email ?? "NoEmail"),
        new Claim("role", "Administrator")
    };

    var token = new JwtSecurityToken(issuer, issuer, claims, expires: DateTime.Now.AddHours(1), signingCredentials: credentials);
    return new JwtSecurityTokenHandler().WriteToken(token);
}


app.MapGet("/vehicles", async (ApplicationDbContext context) =>
    await context.Vehicles.ToListAsync())
    .RequireAuthorization();

app.MapPost("/vehicles", async (Vehicle vehicle, ApplicationDbContext context) =>
{
    context.Vehicles.Add(vehicle);
    await context.SaveChangesAsync();
    return Results.Created($"/vehicles/{vehicle.Id}", vehicle);
}).RequireAuthorization();

app.MapPut("/vehicles/{id}", async (int id, Vehicle input, ApplicationDbContext context) =>
{
    var veiculo = await context.Vehicles.FindAsync(id);
    if (veiculo is null) return Results.NotFound();

    veiculo.Mark = input.Mark;
    veiculo.Model = input.Model;
    veiculo.Year = input.Year;
    await context.SaveChangesAsync();

    return Results.NoContent();
}).RequireAuthorization();

app.MapDelete("/vehicles/{id}", async (int id, ApplicationDbContext context) =>
{
    var veiculo = await context.Vehicles.FindAsync(id);
    if (veiculo is null) return Results.NotFound();

    context.Vehicles.Remove(veiculo);
    await context.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.Run();