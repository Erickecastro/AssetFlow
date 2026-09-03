using System.Text;
using AssetFlow.Api.Authentication;
using AssetFlow.Application.Assets;
using AssetFlow.Application.Common;
using AssetFlow.Application.Departments;
using AssetFlow.Application.Departments.CreateDepartment;
using AssetFlow.Domain.Exceptions;
using AssetFlow.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<CreateDepartmentHandler>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<AssetService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<TokenService>();

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();

if (Encoding.UTF8.GetByteCount(jwtOptions.SigningKey) < 32)
{
    throw new InvalidOperationException(
        "Authentication:SigningKey must contain at least 32 bytes.");
}

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?
            .Error;
        var statusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            DbUpdateException => StatusCodes.Status409Conflict,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidAssetStatusTransitionException => StatusCodes.Status409Conflict,
            InvalidOperationException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        await Results.Problem(
                statusCode: statusCode,
                title: statusCode == 500 ? "Unexpected server error." : exception?.Message)
            .ExecuteAsync(context);
    });
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/token", (LoginRequest request, TokenService tokenService) =>
{
    var token = tokenService.CreateToken(request.Username, request.Password);
    return token is null
        ? Results.Unauthorized()
        : Results.Ok(new TokenResponse(token, "Bearer", 28_800));
}).AllowAnonymous();

var departments = app.MapGroup("/api/departments").RequireAuthorization();
departments.MapPost("/", async (
    CreateDepartmentCommand command,
    CreateDepartmentHandler handler,
    CancellationToken cancellationToken) =>
{
    var result = await handler.HandleAsync(command, cancellationToken);
    return Results.Created($"/api/departments/{result.Id}", result);
});
departments.MapGet("/", (
    DepartmentService service,
    CancellationToken cancellationToken) => service.ListAsync(cancellationToken));

var assets = app.MapGroup("/api/assets").RequireAuthorization();
assets.MapPost("/", async (
    CreateAssetCommand command,
    AssetService service,
    CancellationToken cancellationToken) =>
{
    var result = await service.CreateAsync(command, cancellationToken);
    return Results.Created($"/api/assets/{result.Id}", result);
});
assets.MapGet("/", (
    AssetService service,
    CancellationToken cancellationToken) => service.ListAsync(cancellationToken));
assets.MapGet("/{id:guid}", (
    Guid id,
    AssetService service,
    CancellationToken cancellationToken) => service.GetAsync(id, cancellationToken));
assets.MapPost("/{id:guid}/assign", (
    Guid id,
    MoveAssetCommand command,
    AssetService service,
    TimeProvider timeProvider,
    CancellationToken cancellationToken) =>
    service.AssignAsync(id, command, timeProvider.GetUtcNow(), cancellationToken));
assets.MapPost("/{id:guid}/transfer", (
    Guid id,
    MoveAssetCommand command,
    AssetService service,
    TimeProvider timeProvider,
    CancellationToken cancellationToken) =>
    service.TransferAsync(id, command, timeProvider.GetUtcNow(), cancellationToken));
assets.MapPost("/{id:guid}/return", (
    Guid id,
    ReturnAssetCommand command,
    AssetService service,
    TimeProvider timeProvider,
    CancellationToken cancellationToken) =>
    service.ReturnAsync(id, command, timeProvider.GetUtcNow(), cancellationToken));

app.Run();

public partial class Program;
