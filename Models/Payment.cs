using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LogisticaBroker.Models.Enums;
// Ya no necesitamos los Enums de PaymentType o PaymentStatus aquí
// using LogisticaBroker.Models.Enums; 

namespace LogisticaBroker.Models
{
    /// <summary>
    /// Representa un PAGO REALIZADO (comprobante) asociado a un despacho.
    /// </summary>
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int DispatchId { get; set; }
        public Dispatch? Dispatch { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto Pagado (USD)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; } // Es el monto del comprobante

        [Required(ErrorMessage = "La fecha de pago es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Pago")]
        public DateTime PaidDate { get; set; } // Un pago ya se hizo, la fecha es requerida

        [Display(Name = "Notas")]
        public string? Notes { get; set; }

        // --- RELACIÓN CON EL COMPROBANTE FÍSICO ---
        // Vínculo con el archivo (que subiremos en el controlador)
        [Display(Name = "Comprobante Adjunto")]
        public int? DocumentId { get; set; }
        public Document? Document { get; set; }
        // ------------------------------------------

        // Auditoría
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- CAMPOS ELIMINADOS DE LA PARTE 10 ---
        // public PaymentType PaymentType { get; set; }
        // public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        // public DateTime? DueDate { get; set; }
        [Required(ErrorMessage = "Debe seleccionar un concepto.")]
        [Display(Name = "Concepto del Pago")]
        public PaymentType Concept { get; set; }
    }
}