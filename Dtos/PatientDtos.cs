using System.ComponentModel.DataAnnotations;

namespace PatientBillingAPI.Dtos;

public record PatientCreateDto(
    [Required, MaxLengthAttribute(120)] string Name,
    [Required] DateOnly DateOfBirth
);

public record PatientReadDto(
    int Id,
    string Name,
    DateOnly DateOfBirth
);