using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace warat_0583_l005.Models;

[Table("maintenance_records")]
public class MaintenanceModel
{
    [Key]
    [Column("MaintenanceId")]
    public int MaintenanceId { get; set; }

    [Column("CarId")]
    public int CarId { get; set; }

    [Column("BookingId")]
    public int? BookingId { get; set; }

    [Column("IssueDescription")]
    public string IssueDescription { get; set; } = string.Empty;

    [Column("RepairCost")]
    public decimal RepairCost { get; set; }

    [Column("MaintenanceStatus")]
    public string MaintenanceStatus { get; set; } = "reported";

    [Column("ReportedBy")]
    public string ReportedBy { get; set; } = string.Empty;

    [Column("ResolvedBy")]
    public string? ResolvedBy { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("ResolvedAt")]
    public DateTime? ResolvedAt { get; set; }
}
