using System.ComponentModel.DataAnnotations;

namespace LogisticaBroker.Models.Enums
{
    public enum DocumentType
    {
        // Tipos personalizados que pediste:
        [Display(Name = "FACTURA COMERCIAL")]
        FacturaComercial,
        
        [Display(Name = "PACKING LIST")]
        PackingList,
        
        [Display(Name = "TRADUCCIÓN")]
        Traduccion,
        
        [Display(Name = "AVISO DE LLEGADA")]
        AvisoDeLlegada,
        
        [Display(Name = "PÓLIZA DE SEGURO")]
        PolizaDeSeguro,
        
        [Display(Name = "CONOCIMIENTO DE EMBARQUE (BL)")]
        ConocimientoDeEmbarqueBL,
        
        [Display(Name = "BL LIBERADO (EXPRESS RELEASE)")]
        ExpressRelease,
        
        [Display(Name = "CERTIFICADO DE ORIGEN")]
        CertificadoDeOrigen,
        
        [Display(Name = "MANDATO ELECTRÓNICO")]
        MandatoElectronico,
        
        [Display(Name = "REGISTROS SANITARIOS")]
        RegistrosSanitarios,
        
        [Display(Name = "INSPECTION LIST (FOTOS/VIDEOS)")]
        InspectionList,

        // --- Tipos internos que ya usamos ---
        
        [Display(Name = "COMPROBANTE DE PAGO")]
        PaymentProof, // Usado por el sistema de Liquidaciones (Pagos)
        
        [Display(Name = "FACTURA / COSTO")]
        Invoice, // Usado por el sistema de Liquidaciones (Costos)

        [Display(Name = "OTRO")]
        Other // Un tipo genérico por si acaso
    }
}