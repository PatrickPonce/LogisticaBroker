using System.ComponentModel.DataAnnotations;

namespace LogisticaBroker.Models.Enums
{
    public enum ContractStatus
    {
        [Display(Name = "Pendiente de Firma")]
        Pending,
        [Display(Name = "Firmado")]
        Signed
    }
}
