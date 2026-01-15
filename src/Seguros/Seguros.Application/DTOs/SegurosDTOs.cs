using System.ComponentModel.DataAnnotations;

namespace Seguros.Application.DTOs
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string NumeroIdentificacion { get; set; }
        public string Nombre { get; set; }
        public string ApPaterno { get; set; }
        public string ApMaterno { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string NombreCompleto { get; set; }
        public List<PolizaDto> Polizas { get; set; } = new List<PolizaDto>();
    }

    public class CreateClienteRequest
    {
        [Required]
        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Debe ser exactamente 10 dígitos")]
        public string NumeroIdentificacion { get; set; }
        
        [Required]
        [MaxLength(40)]
        public string Nombre { get; set; }
        
        [Required]
        [MaxLength(40)]
        public string ApPaterno { get; set; }
        
        [Required]
        [MaxLength(40)]
        public string ApMaterno { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Telefono { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        
        [MaxLength(250)]
        public string Direccion { get; set; }
        
        public int? UserId { get; set; }
    }

    public class UpdateClienteRequest
    {
        [MaxLength(40)]
        public string Nombre { get; set; }
        
        [MaxLength(40)]
        public string ApPaterno { get; set; }
        
        [MaxLength(40)]
        public string ApMaterno { get; set; }
        
        [MaxLength(20)]
        public string Telefono { get; set; }
        
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        
        [MaxLength(250)]
        public string Direccion { get; set; }
    }

    public class PolizaDto
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public int TipoPoliza { get; set; }
        public string TipoPolizaDescripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal Monto { get; set; }
        public int Estatus { get; set; }
        public string EstatusDescripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool EsVigente { get; set; }
        public int DiasRestantes { get; set; }
        public ClienteDto Cliente { get; set; }
    }

    public class CreatePolizaRequest
    {
        [Required]
        public int IdCliente { get; set; }
        
        [Required]
        [Range(1, 4, ErrorMessage = "Tipo de póliza debe ser entre 1 y 4")]
        public int TipoPoliza { get; set; }
        
        [Required]
        public DateTime FechaInicio { get; set; }
        
        [Required]
        public DateTime FechaFin { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }
        
        [Required]
        [Range(1, 4, ErrorMessage = "Estatus debe ser entre 1 y 4")]
        public int Estatus { get; set; }
    }

    public class UpdatePolizaRequest
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public decimal? Monto { get; set; }
        public int? Estatus { get; set; }
    }
}