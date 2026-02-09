using System.ComponentModel.DataAnnotations;

namespace PatientBillingAPI.Models;

public class Patient {
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateOnly DateOfBirth { get; set; }

    public List<Invoice> Invoices { get; set; } = new();
}