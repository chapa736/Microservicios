using Auth.WebAPI.Extensions;
using Auth.Infrastructure.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
builder.Services.AddSerilogConfiguration(builder.Configuration);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuraciones personalizadas
builder.Services.AddSwaggerConfiguration();
builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddCorsConfiguration();
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
//{
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
    c.RoutePrefix = string.Empty; // Swagger en la raíz
});
//}

// Middleware pipeline
app.UseSerilogRequestLogging();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors(app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker") ? "AllowAll" : "Production");
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health");
app.MapControllers();

try
{
    Log.Information("Iniciando Auth API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}