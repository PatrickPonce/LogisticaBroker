using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LogisticaBroker.Models.Enums;

namespace LogisticaBroker.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int DispatchId { get; set; }
        public Dispatch? Dispatch { get; set; }

        [Required]
        [Display(Name = "Tipo de Pago")]
        public PaymentType PaymentType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto (USD)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Estado")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha Vencimiento")]
        public DateTime? DueDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Pago")]
        public DateTime? PaidDate { get; set; }

        [Display(Name = "Notas")]
        public string? Notes { get; set; }

        // Auditoría
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}