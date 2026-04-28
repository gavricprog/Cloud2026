using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();

// Data access (PostgreSQL via EF Core)
builder.Services.AddDbContext<SmartApiary.Api.Data.SmartApiaryDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("Default");
    options.UseNpgsql(cs);
});

builder.Services.AddScoped<SmartApiary.Api.Services.ITelemetryService, SmartApiary.Api.Services.TelemetryService>();

// Realtime updates
builder.Services.AddSignalR();

// Frontend dev server (Vite) + SignalR (needs credentials)
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("frontend");

app.MapControllers();
app.MapHub<SmartApiary.Api.Hubs.TelemetryHub>("/hubs/telemetry");

app.Run();
