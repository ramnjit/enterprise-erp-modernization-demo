using LegacyErpMonolith.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore; 

var builder = WebApplication.CreateBuilder(args);

// 1. DEFINING THE CORS POLICY (This was missing!)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi(); 

// Register the AppDbContext using our SQLite connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DataSeeder.SeedData(context); 
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); 
    app.MapScalarApiReference(); 
}

app.UseHttpsRedirection();

// EXPLICIT ROUTING TO CATCH THE 'OPTIONS' SCOUT
app.UseRouting();

// ACTIVATING THE POLICY DEFINED AT THE TOP
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();