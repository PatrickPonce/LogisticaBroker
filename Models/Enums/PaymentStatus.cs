using System.ComponentModel.DataAnnotations;

namespace LogisticaBroker.Models.Enums
{
    public enum PaymentStatus
    {
        [Display(Name = "Pendiente")]
        Pending,
        [Display(Name = "Pagado")]
        Paid,
        [Display(Name = "Anulado")]
        Voided
    }
}