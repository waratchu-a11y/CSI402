using System;

namespace warat_0583_l005.ViewModels
{
    public class LabUserViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string PickupLocation { get; set; } = string.Empty;
        public DateTime PickupDate { get; set; }
        public int RentalDays { get; set; }
        public decimal PricePerDay { get; set; }
    }
}