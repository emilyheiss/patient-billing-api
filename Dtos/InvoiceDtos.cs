using System.ComponentModel.DataAnnotations;
using PatientBillingAPI.Models;

namespace PatientBillingAPI.Dtos;

public record InvoiceCreateDto(
    [Required] int PatientId,
    [Range(0.01, 1_000_000)] decimal Amount
);

public record InvoiceReadDto(
    int Id,
    int PatientId,
    decimal Amount,
    InvoiceStatus Status,
    DateTime CreatedAtUtc,
    DateTime? PaidUtc
);