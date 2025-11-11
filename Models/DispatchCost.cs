using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LogisticaBroker.Models.Enums;

namespace LogisticaBroker.Models
{
    public class DispatchCost
    {
        public int Id { get; set; }

        [Required]
        public int DispatchId { get; set; }
        public Dispatch? Dispatch { get; set; }

        [Required]
        [Display(Name = "Concepto del Costo")]
        public PaymentType Concept { get; set; } // Re-usamos el Enum que ya tenías

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto del Costo (USD)")]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Display(Name = "Notas")]
        public string? Notes { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha Vencimiento")]
        public DateTime? DueDate { get; set; }

        // (CRITERIO 1) Vínculo con el archivo físico (la factura)
        public int? DocumentId { get; set; }
        public Document? Document { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}