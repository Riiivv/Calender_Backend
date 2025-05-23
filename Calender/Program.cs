using Calender.Interface;
using Calender.Models;
using Calender.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<ICalendar, CalendarRepo>();
builder.Services.AddScoped<ICalendarUser, CalendarUserRepo>();
builder.Services.AddScoped<IEventInvitation, EventInvitationRepo>();
builder.Services.AddScoped<IEvent, EventRepo>();
builder.Services.AddScoped<IEventUser, EventUserRepo>();
builder.Services.AddScoped<IUser, UserRepo>();



string connectionstring = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connectionstring, b => b.MigrationsAssembly("Calender")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwTSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwTSettings:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwTSettings:Token"]!)),
            ValidateIssuerSigningKey = true
        };
    });


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
