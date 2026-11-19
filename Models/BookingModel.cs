using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace warat_0583_l005.Models
{
    [Table("bookings")]
    public class BookingModel
    {
        [Key]
        [Column("BookingId")]
        public int BookingId { get; set; }

        [Column("UserId")]
        public string UserId { get; set; }

        [Column("CarModel")]
        public string CarModel { get; set; }

        [Column("PickupLocation")]
        public string PickupLocation { get; set; }

        [Column("PickupDate")]
        public string PickupDate { get; set; }

        [Column("RentalDays")]
        public int RentalDays { get; set; }

        [Column("PricePerDay")]
        public decimal PricePerDay { get; set; }

        [Column("TotalPrice")]
        public decimal TotalPrice { get; set; }

        [Column("BookingDate")]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Column("HasInsurance")]
        public bool HasInsurance { get; set; }     // ประกันภัย (True/False)

        [Column("HasBabySeat")]
        public bool HasBabySeat { get; set; }      // คาร์ซีทเด็ก (True/False)

        [Column("AddonTotal")]
        public decimal AddonTotal { get; set; }    // ราคารวมบริการเสริม

        [Column("CarId")]
        public int? CarId { get; set; }

        [Column("ReturnLocation")]
        public string? ReturnLocation { get; set; }

        [Column("PickupTime")]
        public TimeSpan? PickupTime { get; set; }

        [Column("ReturnDate")]
        public DateTime? ReturnDate { get; set; }

        [Column("ReturnTime")]
        public TimeSpan? ReturnTime { get; set; }

        [Column("DepositAmount")]
        public decimal DepositAmount { get; set; }

        [Column("BookingStatus")]
        public string BookingStatus { get; set; } = "pending";

        [Column("DiscountAmount")]
        public decimal DiscountAmount { get; set; }

        [Column("PromotionCode")]
        public string? PromotionCode { get; set; }

        [Column("LicenseImagePath")]
        public string? LicenseImagePath { get; set; }

        [Column("LicenseStatus")]
        public string LicenseStatus { get; set; } = "pending";

        [Column("LicenseReviewNote")]
        public string? LicenseReviewNote { get; set; }

        [Column("ReturnMileage")]
        public int? ReturnMileage { get; set; }

        [Column("ReturnImagePaths")]
        public string? ReturnImagePaths { get; set; }

        [Column("ReturnSubmittedAt")]
        public DateTime? ReturnSubmittedAt { get; set; }
    }
}