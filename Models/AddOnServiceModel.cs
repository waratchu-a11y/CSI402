using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace warat_0583_l005.Models;

[Table("add_on_services")]
public class AddOnServiceModel
{
    [Key]
    [Column("ServiceId")]
    public int ServiceId { get; set; }

    [Column("ServiceCode")]
    public string ServiceCode { get; set; } = string.Empty;

    [Column("ServiceName")]
    public string ServiceName { get; set; } = string.Empty;

    [Column("PricePerDay")]
    public decimal PricePerDay { get; set; }

    [Column("IsActive")]
    public bool IsActive { get; set; } = true;
}
