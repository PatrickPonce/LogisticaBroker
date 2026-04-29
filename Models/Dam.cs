using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LogisticaBroker.Models.Enums;

namespace LogisticaBroker.Models
{
    /// <summary>
    /// Borrador o declaración oficial de la DAM (Declaración Aduanera de Mercancías).
    /// CIF = FOB + Flete + Seguro (calculado automáticamente).
    /// </summary>
    public class Dam
    {
        public int Id { get; set; }

        [Required]
        public int DispatchId { get; set; }
        public Dispatch? Dispatch { get; set; }

        // --- Valores base ---
        [Required(ErrorMessage = "El valor FOB es obligatorio.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Valor FOB (USD)")]
        [Range(0, double.MaxValue)]
        public decimal FobValue { get; set; }

        [Required(ErrorMessage = "El flete es obligatorio.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Flete (USD)")]
        [Range(0, double.MaxValue)]
        public decimal FreightValue { get; set; }

        [Required(ErrorMessage = "El seguro es obligatorio.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Seguro (USD)")]
        [Range(0, double.MaxValue)]
        public decimal InsuranceValue { get; set; }

        /// <summary>CIF calculado: FobValue + FreightValue + InsuranceValue.</summary>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Valor CIF (USD)")]
        public decimal CifValue => FobValue + FreightValue + InsuranceValue;

        // --- Campos aduaneros ---
        [Display(Name = "Régimen Aduanero")]
        [MaxLength(50)]
        public string? CustomsRegime { get; set; }

        [Display(Name = "Aduana de Ingreso")]
        [MaxLength(100)]
        public string? EntryCustomsOffice { get; set; }

        [Display(Name = "Notas / Observaciones")]
        public string? Notes { get; set; }

        [Display(Name = "Estado")]
        public DamStatus Status { get; set; } = DamStatus.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
