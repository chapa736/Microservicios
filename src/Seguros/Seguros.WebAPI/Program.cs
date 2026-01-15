using Seguros.WebAPI.Extensions;
using Seguros.Infrastructure.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
builder.Services.AddSerilogConfiguration(builder.Configuration);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuraciones personalizadas
builder.Services.AddSwaggerConfiguration();
builder.Services.AddSegurosServices(builder.Configuration);
builder.Services.AddCorsConfiguration();
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Finasist Seguros API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Middleware pipeline
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "Production");
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health");
app.MapControllers();

try
{
    Log.Information("Iniciando Finasist Seguros API");
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
