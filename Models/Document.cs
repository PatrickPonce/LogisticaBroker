using System.ComponentModel.DataAnnotations;
using LogisticaBroker.Models.Enums;

namespace LogisticaBroker.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tipo de Documento")]
        public DocumentType DocType { get; set; } = DocumentType.Other;

        [Required]
        [Display(Name = "Nombre del Archivo")]
        public string FileName { get; set; } = string.Empty;

        // Esta es la ruta relativa donde se guardará en el servidor (ej: "/uploads/doc1.pdf")
        public string FilePath { get; set; } = string.Empty;

        public string? ContentType { get; set; } // Ej: "application/pdf"
        public long FileSize { get; set; } // En bytes

        public string? Notes { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Relación con Despacho
        [Required]
        public int DispatchId { get; set; }
        public Dispatch? Dispatch { get; set; }

        // Relación muchos-a-muchos con timeline
        public ICollection<DispatchTimeline> Timelines { get; set; } = new List<DispatchTimeline>();
    }
}