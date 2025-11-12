using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LogisticaBroker.Models.Enums;

namespace LogisticaBroker.Models
{
    public class Dispatch
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El Nro. de Despacho es obligatorio.")]
        [Display(Name = "Nro. Despacho")]
        public string DispatchNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El BL Number es obligatorio.")]
        [Display(Name = "BL Number")]
        public string? BLNumber { get; set; }

        // Foreign Key al Cliente
        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        [Display(Name = "Cliente")]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        [Required(ErrorMessage = "El Proveedor es obligatorio.")]
        [Display(Name = "Proveedor")]
        public string? Supplier { get; set; }

        [Required(ErrorMessage = "La Línea Naviera es obligatoria.")]
        [Display(Name = "Línea Naviera")]
        public string? ShippingLine { get; set; }

        [Required(ErrorMessage = "La Fecha de Llegada es obligatoria.")]
        [Display(Name = "Fecha de Llegada")]
        [DataType(DataType.Date)]
        public DateTime? ArrivalDate { get; set; }

        [Display(Name = "Canal")]
        public DispatchChannel Channel { get; set; } = DispatchChannel.Pending;

        [Display(Name = "Estado")]
        public DispatchStatus Status { get; set; } = DispatchStatus.Pending;

        [Required(ErrorMessage = "El Nro. de Contenedor es obligatorio.")]
        [Display(Name = "Nro. Contenedor")]
        public string? ContainerNumber { get; set; }

        [Required(ErrorMessage = "El Puerto es obligatorio.")]
        [Display(Name = "Puerto")]
        public string? Port { get; set; }

        [Required(ErrorMessage = "El Peso es obligatorio.")]
        [Column(TypeName = "decimal(18,2)")] // Importante para monedas/pesos en Postgres
        [Display(Name = "Peso (Kg)")]
        public decimal? Weight { get; set; }

        [Required(ErrorMessage = "El Valor es obligatorio.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Valor (USD)")]
        public decimal? Value { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<DispatchTimeline> Timeline { get; set; } = new List<DispatchTimeline>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public ICollection<DispatchCost> Costs { get; set; } = new List<DispatchCost>();
    }
}