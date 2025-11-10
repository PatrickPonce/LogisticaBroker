using System.ComponentModel.DataAnnotations;

namespace LogisticaBroker.Models.Enums
{
    public enum DocumentType
    {
        [Display(Name = "Otro")]
        Other,
        [Display(Name = "Factura Comercial")]
        Invoice,
        [Display(Name = "Packing List")]
        PackingList,
        [Display(Name = "Bill of Lading (BL)")]
        BillOfLading,
        [Display(Name = "Certificado de Origen")]
        CertificateOfOrigin,
        [Display(Name = "Declaración Aduanera (DAM)")]
        CustomsDeclaration,
        [Display(Name = "Comprobante de Pago")]
        PaymentProof
    }
}