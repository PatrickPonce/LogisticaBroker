using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LogisticaBroker.Models
{
    // Extendemos IdentityUser para añadir nuestros campos personalizados
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "Nombre Completo")]
        public string FullName { get; set; } = string.Empty;

        // Estos campos son opcionales, útiles si el usuario es un Cliente con acceso al sistema
        [Display(Name = "Nombre de Empresa")]
        public string? CompanyName { get; set; }

        [Display(Name = "RUC")]
        public string? RUC { get; set; }

        [Display(Name = "Dirección")]
        public string? Address { get; set; }
        
        // Relación: Un usuario puede haber creado muchos clientes (si es admin)
        // O un usuario puede SER un cliente (dependerá de tu lógica final, por ahora lo dejamos flexible)
    }
}