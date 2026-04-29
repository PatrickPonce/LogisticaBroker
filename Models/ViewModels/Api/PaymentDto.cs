using System.ComponentModel.DataAnnotations;
using LogisticaBroker.Models.Enums;

namespace LogisticaBroker.Models.ViewModels.Api
{
    public record PaymentDto(
        int Id,
        int DispatchId,
        decimal Amount,
        DateTime PaidDate,
        string? Notes,
        string Concept,
        DateTime CreatedAt
    );

    public record PaymentCreateDto(
        [Required] int DispatchId,
        [Required][Range(0.01, double.MaxValue)] decimal Amount,
        [Required] DateTime PaidDate,
        string? Notes,
        [Required] PaymentType Concept
    );

    public record DispatchCostDto(
        int Id,
        int DispatchId,
        string Concept,
        decimal Amount,
        string? Notes,
        DateTime? DueDate,
        DateTime CreatedAt
    );

    public record DispatchCostCreateDto(
        [Required] int DispatchId,
        [Required] PaymentType Concept,
        [Required][Range(0.01, double.MaxValue)] decimal Amount,
        string? Notes,
        DateTime? DueDate
    );
}
