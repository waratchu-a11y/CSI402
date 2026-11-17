using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace warat_0583_l005.Models;

[Table("car_inspections")]
public class CarInspectionModel
{
    [Key]
    [Column("InspectionId")]
    public int InspectionId { get; set; }

    [Column("BookingId")]
    public int BookingId { get; set; }

    [Column("CarId")]
    public int CarId { get; set; }

    [Column("InspectionStage")]
    public string InspectionStage { get; set; } = string.Empty;

    [Column("Mileage")]
    public int Mileage { get; set; }

    [Column("FuelLevel")]
    public int FuelLevel { get; set; }

    [Column("ConditionNotes")]
    public string? ConditionNotes { get; set; }

    [Column("DamageAmount")]
    public decimal DamageAmount { get; set; }

    [Column("InspectionStatus")]
    public string InspectionStatus { get; set; } = "complete";

    [Column("InspectedBy")]
    public string InspectedBy { get; set; } = string.Empty;

    [Column("InspectedAt")]
    public DateTime InspectedAt { get; set; } = DateTime.Now;
}
