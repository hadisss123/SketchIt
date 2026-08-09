using Microsoft.EntityFrameworkCore;
using SketchIt.Api.Hubs;
using System.Collections.Concurrent;
using SketchIt.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddDbContext<SketchItDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddSignalR();

builder.Services.AddScoped<RoomService>();
builder.Services.AddSingleton<RoomState>();

var app = builder.Build();

app.UseCors("AllowReactDev");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapHub<GameHub>("/gamehub");



app.Run();
