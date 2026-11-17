using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace warat_0583_l005.Models;

[Table("system_settings")]
public class SystemSettingModel
{
    [Key]
    [Column("SettingKey")]
    public string SettingKey { get; set; } = string.Empty;

    [Column("SettingValue")]
    public string SettingValue { get; set; } = string.Empty;
}
