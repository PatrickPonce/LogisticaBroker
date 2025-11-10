using System.ComponentModel.DataAnnotations;

namespace LogisticaBroker.Models.Enums
{
    public enum DispatchStatus
    {
        [Display(Name = "Pendiente")]
        Pending,
        [Display(Name = "Documentación")]
        Documentation,
        [Display(Name = "En Tránsito")]
        InTransit,
        [Display(Name = "Arribado")]
        Arrived,
        [Display(Name = "En Aduana")]
        Customs,
        [Display(Name = "Liberado")]
        Released,
        [Display(Name = "Completado")]
        Completed
    }
}