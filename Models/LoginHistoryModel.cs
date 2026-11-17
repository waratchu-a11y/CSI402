using System;

namespace warat_0583_l005.Models
{
    public class LoginHistoryModel
    {
        public int LoginHistoryId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime LoggedInAtUtc { get; set; }
    }
}