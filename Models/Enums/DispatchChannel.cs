using System.ComponentModel.DataAnnotations;

namespace LogisticaBroker.Models.Enums

{
    public enum DispatchChannel
    {
        [Display(Name = "Pendiente")] // <--- ESTA LÍNEA
        Pending,
        
        [Display(Name = "Verde")] // <--- ESTA LÍNEA
        Green,
        
        [Display(Name = "Naranja")] // <--- ESTA LÍNEA
        Orange,
        
        [Display(Name = "Rojo")] // <--- ESTA LÍNEA
        Red
    }
}