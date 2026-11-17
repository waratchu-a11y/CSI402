using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace warat_0583_l005.Models;

[Table("payments")]
public class PaymentModel
{
    [Key]
    [Column("PaymentId")]
    public int PaymentId { get; set; }

    [Column("BookingId")]
    public int BookingId { get; set; }

    [Column("PaymentType")]
    public string PaymentType { get; set; } = "deposit";

    [Column("Amount")]
    public decimal Amount { get; set; }

    [Column("PaymentStatus")]
    public string PaymentStatus { get; set; } = "pending";

    [Column("ProofFileName")]
    public string? ProofFileName { get; set; }

    [Column("ReferenceNumber")]
    public string? ReferenceNumber { get; set; }

    [Column("Note")]
    public string? Note { get; set; }

    [Column("ReviewedBy")]
    public string? ReviewedBy { get; set; }

    [Column("ReviewedAt")]
    public DateTime? ReviewedAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
