using System;

namespace warat_0583_l005.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        
        // 📌 เติม ? หลัง string ทุกตัว เพื่อรองรับค่า NULL จาก Database เก่า
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        
        public string? IdCard { get; set; }
        public string? DriverLicense { get; set; }
        public bool IsActive { get; set; } = true;

        public string? Role { get; set; } = "Customer";
        public DateTime RegisterDate { get; set; } = DateTime.Now;
    }
}