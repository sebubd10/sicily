using BasicCommerce.Application.Common.Behaviors;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Infrastructure;
using BasicCommerce.Infrastructure.Middleware;
using BasicCommerce.Infrastructure.Persistence;
using BasicCommerce.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
var authBuilder = builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;   // keep "role" as "role", not the long ClaimTypes.Role URI
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer           = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidateAudience         = true,
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            ClockSkew                = TimeSpan.Zero
        };
    });

var googleClientId = builder.Configuration["OAuth:Google:ClientId"];
if (!string.IsNullOrWhiteSpace(googleClientId))
{
    authBuilder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId     = googleClientId;
        options.ClientSecret = builder.Configuration["OAuth:Google:ClientSecret"]!;
        options.CallbackPath = "/auth/google/callback";
    });
}

var microsoftClientId = builder.Configuration["OAuth:Microsoft:ClientId"];
if (!string.IsNullOrWhiteSpace(microsoftClientId))
{
    authBuilder.AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, options =>
    {
        options.ClientId     = microsoftClientId;
        options.ClientSecret = builder.Configuration["OAuth:Microsoft:ClientSecret"]!;
        options.CallbackPath = "/auth/microsoft/callback";
    });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AllAuthenticated",     p => p.RequireAuthenticatedUser());
    options.AddPolicy("ChainAdminOnly",       p => p.RequireClaim("role", "ChainAdmin", "SystemAdmin"));
    options.AddPolicy("StoreManagerAndAbove", p => p.RequireClaim("role", "StoreManager", "ChainAdmin", "SystemAdmin"));
    options.AddPolicy("SupervisorAndAbove",   p => p.RequireClaim("role", "Supervisor", "StoreManager", "ChainAdmin", "SystemAdmin"));
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
{
    c.SwaggerDoc("v1", new() { Title = "BasicCommerce Backoffice API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Name         = "Authorization",
        Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            []
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandling();
app.UseCors("BasicCommerceCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.Services.MigrateAndSeedAsync();

app.Run();
