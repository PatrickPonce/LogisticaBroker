using System.ComponentModel.DataAnnotations;
using LogisticaBroker.Models.Enums;

namespace LogisticaBroker.Models
{
    public class DispatchTimeline
    {
        public int Id { get; set; }

        [Required]
        public int DispatchId { get; set; }
        public Dispatch? Dispatch { get; set; }

        [Required]
        [Display(Name = "Estado Nuevo")]
        public DispatchStatus Status { get; set; }

        [Display(Name = "Notas")]
        public string? Notes { get; set; }

        // Auditoría: Quién hizo el cambio
        public string? ChangedById { get; set; }
        public ApplicationUser? ChangedBy { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}