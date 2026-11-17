using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace warat_0583_l005.Models;

[Table("promotions")]
public class PromotionModel
{
    [Key]
    [Column("PromotionId")]
    public int PromotionId { get; set; }

    [Column("PromotionName")]
    public string PromotionName { get; set; } = string.Empty;

    [Column("PromotionCode")]
    public string? PromotionCode { get; set; }

    [Column("ConditionType")]
    public string ConditionType { get; set; } = string.Empty;

    [Column("DiscountPercent")]
    public decimal DiscountPercent { get; set; }

    [Column("StartDate")]
    public DateTime? StartDate { get; set; }

    [Column("EndDate")]
    public DateTime? EndDate { get; set; }

    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
