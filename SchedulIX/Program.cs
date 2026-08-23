using SchedulIX.Data;
using SchedulIX.Interfaces;
using SchedulIX.Services;
using SchedulIX.Repositories.Interfaces;
using SchedulIX.Repositories.Implementations;
using SchedulIX.Services.Interfaces;
using SchedulIX.Services.Implementations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database - Using SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\mssqllocaldb;Database=schedulix_db;Trusted_Connection=true;";

builder.Services.AddDbContext<ScheduleDbContext>(options =>
    options.UseSqlServer(connectionString)
);

// Register Services
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IExportService, ExportService>();

// Register Repositories
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();

// Configure CORS for Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseFileServer();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
