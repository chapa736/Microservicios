using System.ComponentModel.DataAnnotations;

namespace Seguros.Domain.Entities
{
    public class Cliente
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(10)]
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
        [MaxLength(100)]
        public string Email { get; set; }
        
        [MaxLength(250)]
        public string Direccion { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        public DateTime? FechaActualizacion { get; set; }
        
        public int? UserId { get; set; }
        
        // Navigation properties
        public virtual ICollection<Poliza> Polizas { get; set; } = new List<Poliza>();
        
        public string NombreCompleto => $"{Nombre} {ApPaterno} {ApMaterno}";
    }

    public class Poliza
    {
        public int Id { get; set; }
        
        public int IdCliente { get; set; }
        
        public int TipoPoliza { get; set; }
        
        public DateTime FechaInicio { get; set; }
        
        public DateTime FechaFin { get; set; }
        
        public decimal Monto { get; set; }
        
        public int Estatus { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Cliente Cliente { get; set; }
        
        public bool EsVigente => DateTime.Now >= FechaInicio && DateTime.Now <= FechaFin && Estatus == 1;
        
        public int DiasRestantes => (FechaFin - DateTime.Now).Days;
    }
}