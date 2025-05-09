using Calender.Interface;
using Calender.Models;
using Calender.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<ICalendar, CalendarRepo>();

string connectionstring = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connectionstring, b => b.MigrationsAssembly("CalenderApp.API")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
