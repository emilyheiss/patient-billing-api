using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientBillingAPI.Models;

public enum InvoiceStatus
{
    Pending = 0,
    Paid = 1
}

public class Invoice
{
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    public Patient? Patient { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, 1_000_000)]
    public decimal Amount { get; set; }

    [Required]
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

    public DateTime CreatedAtUtc { get; set;} = DateTime.UtcNow;
    public DateTime? PaidAtUtc { get; set; }
}