using System.ComponentModel.DataAnnotations;

namespace LogisticaBroker.Models.Enums
{
    public enum DamStatus
    {
        [Display(Name = "Borrador")]
        Draft,
        [Display(Name = "Final")]
        Final
    }
}
