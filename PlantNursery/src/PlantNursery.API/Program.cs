using PlantNursery.API.Extensions;
using PlantNursery.API.Middleware;
using PlantNursery.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApiControllers();

builder.Services.AddApiSwagger();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();


// Database / Identity seed
//await app.SeedIdentityAsync();

// Middleware
app.UseApiSwagger();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("ReactFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();