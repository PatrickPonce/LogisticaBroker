using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LogisticaBroker.Models.Enums;

namespace LogisticaBroker.Models
{
    public class Dispatch
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nro. Despacho")]
        public string DispatchNumber { get; set; } = string.Empty;

        [Display(Name = "BL Number")]
        public string? BLNumber { get; set; }

        // Foreign Key al Cliente
        [Required]
        [Display(Name = "Cliente")]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        [Display(Name = "Proveedor")]
        public string? Supplier { get; set; }

        [Display(Name = "Línea Naviera")]
        public string? ShippingLine { get; set; }

        [Display(Name = "Fecha de Llegada")]
        [DataType(DataType.Date)]
        public DateTime? ArrivalDate { get; set; }

        [Display(Name = "Canal")]
        public DispatchChannel Channel { get; set; } = DispatchChannel.Pending;

        [Display(Name = "Estado")]
        public DispatchStatus Status { get; set; } = DispatchStatus.Pending;

        [Display(Name = "Nro. Contenedor")]
        public string? ContainerNumber { get; set; }

        [Display(Name = "Puerto")]
        public string? Port { get; set; }

        [Column(TypeName = "decimal(18,2)")] // Importante para monedas/pesos en Postgres
        public decimal? Weight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Value { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<DispatchTimeline> Timeline { get; set; } = new List<DispatchTimeline>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}