using System.ComponentModel.DataAnnotations;

namespace LogisticaBroker.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        // El usuario que recibirá la notificación
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        // Para saber si ya la vio
        public bool IsRead { get; set; } = false;

        // Opcional: Para enlazarla y poder hacer clic e ir al despacho
        public int? RelatedDispatchId { get; set; }
        public Dispatch? RelatedDispatch { get; set; }

        public string Type { get; set; } = "info"; // info, success, warning, danger

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}