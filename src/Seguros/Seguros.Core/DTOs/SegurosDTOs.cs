using System.ComponentModel.DataAnnotations;
using Seguros.Core.Enums;

namespace Seguros.Core.DTOs
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
    }

    public class CreateClienteDto
    {
        [Required(ErrorMessage = "El número de identificación es obligatorio")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "El número de identificación debe tener exactamente 10 dígitos")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El número de identificación debe ser numérico de 10 dígitos")]
        public string NumeroIdentificacion { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(40, ErrorMessage = "El nombre no puede exceder 40 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre no debe contener números ni caracteres especiales")]
        public string Nombre { get; set; }
        
        [Required(ErrorMessage = "El apellido paterno es obligatorio")]
        [MaxLength(40, ErrorMessage = "El apellido paterno no puede exceder 40 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido paterno no debe contener números ni caracteres especiales")]
        public string ApPaterno { get; set; }
        
        [Required(ErrorMessage = "El apellido materno es obligatorio")]
        [MaxLength(40, ErrorMessage = "El apellido materno no puede exceder 40 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido materno no debe contener números ni caracteres especiales")]
        public string ApMaterno { get; set; }
        
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [MaxLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        public string Telefono { get; set; }
        
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [MaxLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
        public string Email { get; set; }
        
        [MaxLength(250, ErrorMessage = "La dirección no puede exceder 250 caracteres")]
        public string Direccion { get; set; }
    }

    public class PolizaDto
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public TipoPoliza TipoPoliza { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal Monto { get; set; }
        public EstatusPoliza Estatus { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool EsVigente { get; set; }
        public ClienteDto Cliente { get; set; }
    }

    public class CreatePolizaDto
    {
        [Required(ErrorMessage = "El cliente es obligatorio")]
        public int IdCliente { get; set; }
        
        [Required(ErrorMessage = "El tipo de póliza es obligatorio")]
        public TipoPoliza TipoPoliza { get; set; }
        
        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        public DateTime FechaInicio { get; set; }
        
        [Required(ErrorMessage = "La fecha de expiración es obligatoria")]
        public DateTime FechaFin { get; set; }
        
        [Required(ErrorMessage = "El monto asegurado es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto asegurado debe ser un valor positivo")]
        public decimal Monto { get; set; }
        
        [Required(ErrorMessage = "El estatus es obligatorio")]
        public EstatusPoliza Estatus { get; set; }
    }

    public class UpdateClienteDto
    {
        public int IdCliente { get; set; }
        [MaxLength(250, ErrorMessage = "La dirección no puede exceder 250 caracteres")]
        public string Direccion { get; set; }
        
        [MaxLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        public string Telefono { get; set; }
    }
}