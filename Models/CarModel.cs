using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace warat_0583_l005.Models
{
    [Table("cars")]
    public class CarModel
    {
        [Key]
        [Column("CarId")]
        public int CarId { get; set; }

        [Column("CarName")]
        public string CarName { get; set; }

        [Column("PricePerDay")]
        public decimal PricePerDay { get; set; }

        [Column("ImageUrl")]
        public string ImageUrl { get; set; }

        [Column("Status")]
        public string Status { get; set; } = "ว่าง";

        [Column("Details")]
        public string Details { get; set; }

        [Column("TransmissionType")]
        public string? TransmissionType { get; set; }

        [Column("Seats")]
        public int? Seats { get; set; }

        [Column("Location")]
        public string? Location { get; set; }
    }
}