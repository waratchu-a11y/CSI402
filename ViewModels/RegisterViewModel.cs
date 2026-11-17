namespace warat_0583_l005.ViewModels
{
    public class RegisterViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        
        public string IdCard { get; set; } = string.Empty;
        
        // 📌 เพิ่มบรรทัดนี้เพื่อแก้ Error ในหน้า Register.cshtml
        public string DriverLicense { get; set; } = string.Empty;
    }
}