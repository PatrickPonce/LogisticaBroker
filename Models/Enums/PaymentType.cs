using System.ComponentModel.DataAnnotations;

namespace LogisticaBroker.Models.Enums
{
    public enum PaymentType
    {
        [Display(Name = "Otro")]
        Other,
        [Display(Name = "Derechos Aduaneros")]
        CustomsDuties,
        [Display(Name = "Impuesto de Importación")]
        ImportTax,
        [Display(Name = "Almacenaje")]
        StorageFees,
        [Display(Name = "Transporte Local")]
        Transport,
        [Display(Name = "Servicios Logísticos")]
        ServiceFees,
        [Display(Name = "Gastos Operativos")]
        OperationalExpenses
    }
}