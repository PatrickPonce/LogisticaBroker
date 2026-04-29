using System.ComponentModel.DataAnnotations;
using LogisticaBroker.Models.Enums;

namespace LogisticaBroker.Models.ViewModels.Api
{
    public record DispatchDto(
        int Id,
        string DispatchNumber,
        string? TrackingCode,
        string? BLNumber,
        int ClientId,
        string? ClientName,
        string? Supplier,
        string? ShippingLine,
        DateTime? ArrivalDate,
        string Channel,
        string Status,
        string? ContainerNumber,
        string? Port,
        decimal? Weight,
        decimal? Value,
        string? TariffCode,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    /// <summary>TrackingCode se autogenera en el backend (ORD-YYYY-XXXX). No se recibe.</summary>
    public record DispatchCreateDto(
        string? BLNumber,
        [Required] int ClientId,
        [Required] string? Supplier,
        [Required] string? ShippingLine,
        [Required] DateTime? ArrivalDate,
        DispatchChannel Channel,
        [Required] string? ContainerNumber,
        [Required] string? Port,
        [Range(0.01, double.MaxValue)] decimal? Weight,
        [Range(0.01, double.MaxValue)] decimal? Value
    );

    public record DispatchUpdateDto(
        string? BLNumber,
        string? Supplier,
        string? ShippingLine,
        DateTime? ArrivalDate,
        DispatchChannel? Channel,
        DispatchStatus? Status,
        string? ContainerNumber,
        string? Port,
        [Range(0.01, double.MaxValue)] decimal? Weight,
        [Range(0.01, double.MaxValue)] decimal? Value
    );

    /// <summary>T35: Asignar partida arancelaria (exactamente 10 dígitos numéricos).</summary>
    public record AssignTariffDto(
        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "La partida arancelaria debe tener exactamente 10 dígitos numéricos.")]
        string TariffCode
    );
}
