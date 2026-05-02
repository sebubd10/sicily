using System.Text;
using BasicCommerce.Application.Common.Behaviors;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Infrastructure;
using BasicCommerce.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(BasicCommerce.Application.Features.Auth.Commands.LoginCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(
    typeof(BasicCommerce.Application.Features.Auth.Commands.LoginCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

var jwtKey = builder.Configuration["Jwt:SecretKey"]!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ChainAdminOnly", policy =>
        policy.RequireClaim("role", "ChainAdmin", "SystemAdmin"));
    options.AddPolicy("StoreManagerAndAbove", policy =>
        policy.RequireClaim("role", "StoreManager", "ChainAdmin", "SystemAdmin"));
    options.AddPolicy("SupervisorAndAbove", policy =>
        policy.RequireClaim("role", "Supervisor", "StoreManager", "ChainAdmin", "SystemAdmin"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("BasicCommerceCors", policy =>
    {
        var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new() { Title = "BasicCommerce Backoffice API", Version = "v1" }));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("BasicCommerceCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
