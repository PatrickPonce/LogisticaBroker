using System.ComponentModel.DataAnnotations;

namespace LogisticaBroker.Models.ViewModels.Api
{
    public record ClientDto(
        int Id,
        string CompanyName,
        string RUC,
        string Email,
        string? Phone,
        string? Address,
        string? ContactPerson,
        string? Notes,
        DateTime CreatedAt
    );

    public record ClientCreateDto(
        [Required] string CompanyName,
        [Required][StringLength(11, MinimumLength = 11)] string RUC,
        [Required][EmailAddress] string Email,
        [Required] string? Phone,
        [Required] string? Address,
        [Required] string? ContactPerson,
        string? Notes
    );

    public record ClientUpdateDto(
        [Required] string CompanyName,
        [Required][EmailAddress] string Email,
        [Required] string? Phone,
        [Required] string? Address,
        [Required] string? ContactPerson,
        string? Notes
    );
}
