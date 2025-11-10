using System.ComponentModel.DataAnnotations;

namespace LogisticaBroker.Models
{
    public class CalendarEvent
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Título del Evento")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Fecha y Hora Inicio")]
        public DateTime Start { get; set; }

        [Display(Name = "Fecha y Hora Fin")]
        public DateTime? End { get; set; }

        // Opcional: Para colorear eventos según tipo
        public string Color { get; set; } = "#3788d8"; // Azul por defecto

        // Relación opcional con Despacho
        [Display(Name = "Despacho Relacionado")]
        public int? DispatchId { get; set; }
        public Dispatch? Dispatch { get; set; }

        // Auditoría
        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsCompleted { get; set; } = false;
    }
}