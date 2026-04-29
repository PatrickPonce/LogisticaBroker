using System.ComponentModel.DataAnnotations;
using LogisticaBroker.Models.Enums;

namespace LogisticaBroker.Models.ViewModels.Api
{
    public record DamDto(
        int Id,
        int DispatchId,
        decimal FobValue,
        decimal FreightValue,
        decimal InsuranceValue,
        decimal CifValue,
        string? CustomsRegime,
        string? EntryCustomsOffice,
        string? Notes,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    /// <summary>T42: Registrar borrador DAM.</summary>
    public record DamCreateDto(
        [Required] int DispatchId,
        [Required][Range(0, double.MaxValue)] decimal FobValue,
        [Required][Range(0, double.MaxValue)] decimal FreightValue,
        [Required][Range(0, double.MaxValue)] decimal InsuranceValue,
        [MaxLength(50)] string? CustomsRegime,
        [MaxLength(100)] string? EntryCustomsOffice,
        string? Notes
    );

    public record DamUpdateDto(
        [Range(0, double.MaxValue)] decimal? FobValue,
        [Range(0, double.MaxValue)] decimal? FreightValue,
        [Range(0, double.MaxValue)] decimal? InsuranceValue,
        [MaxLength(50)] string? CustomsRegime,
        [MaxLength(100)] string? EntryCustomsOffice,
        string? Notes
    );
}
