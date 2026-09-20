using PlantNursery.API.Extensions;
using PlantNursery.API.Middleware;
using PlantNursery.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApiControllers();

builder.Services.AddApiSwagger();

var app = builder.Build();

// Database / Identity seed
//await app.SeedIdentityAsync();

// Middleware
app.UseApiSwagger();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();