using System.ComponentModel.DataAnnotations;

namespace LogisticaBroker.Models
{
    public class Client
    {
        public int Id { get; set; } // Usamos int para PK por simplicidad, puede ser Guid si prefieres

        [Required(ErrorMessage = "El nombre de la empresa es obligatorio.")]
        [Display(Name = "Nombre de Empresa")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RUC es obligatorio")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "El RUC debe tener 11 dígitos")]
        public string RUC { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [Display(Name = "Dirección")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "La persona de contacto es obligatoria.")]
        [Display(Name = "Persona de Contacto")]
        public string? ContactPerson { get; set; }

        [Display(Name = "Notas")]
        public string? Notes { get; set; }

        // Auditoría básica
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relación opcional: Si este cliente tiene un usuario para acceder al sistema
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // Relación: Un cliente tiene muchos despachos
        public ICollection<Dispatch> Dispatches { get; set; } = new List<Dispatch>();
    }
}