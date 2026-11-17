using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using warat_0583_l005.Models;
using warat_0583_l005.Services;

namespace warat_0583_l005.Controllers;

[Authorize]
public class ManagementController : Controller
{
    private static readonly string[] AvailableRoles = ["Customer", "Employee", "Manager", "Finance", "Service", "Admin"];
    private readonly Csi402dbContext _db;

    public ManagementController(Csi402dbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (!RoleAccess.HasRole(_db, User, "Manager", "Admin"))
        {
            return Forbid();
        }

        var payments = _db.Payments.Where(payment =>
            payment.PaymentStatus == "verified" || payment.PaymentStatus == "completed").ToList();
        ViewBag.CarCount = _db.Cars.Count();
        ViewBag.BookingCount = _db.Bookings.Count();
        ViewBag.ActiveBookingCount = _db.Bookings.Count(booking =>
            booking.BookingStatus == "confirmed" || booking.BookingStatus == "rented");
        ViewBag.UserCount = _db.UserModels.Count();
        ViewBag.NetReceived = payments.Sum(payment =>
            payment.PaymentType == "refund" ? -payment.Amount :
            payment.PaymentType == "deduction" ? 0m : payment.Amount);
        ViewBag.RecentPayments = payments.OrderByDescending(payment => payment.CreatedAt).Take(20).ToList();
        return View();
    }

    [HttpGet]
    public IActionResult Promotions()
    {
        if (!RoleAccess.HasRole(_db, User, "Manager", "Admin"))
        {
            return Forbid();
        }

        return View(_db.Promotions.OrderBy(promotion => promotion.PromotionName).ToList());
    }

    [HttpGet]
    public IActionResult AddOnServices()
    {
        if (!RoleAccess.HasRole(_db, User, "Manager", "Admin"))
        {
            return Forbid();
        }

        return View(_db.AddOnServices.OrderBy(service => service.ServiceId).ToList());
    }

    [HttpGet]
    public IActionResult Settings()
    {
        if (!RoleAccess.HasRole(_db, User, "Admin"))
        {
            return Forbid();
        }

        ViewBag.Settings = _db.SystemSettings.ToDictionary(setting => setting.SettingKey, setting => setting.SettingValue);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveBankSettings(string bankName, string bankAccount, string bankAccountName)
    {
        if (!RoleAccess.HasRole(_db, User, "Admin"))
        {
            return Forbid();
        }

        if (bankName?.Length > 100 || bankAccount?.Length > 100 || bankAccountName?.Length > 150)
        {
            return BadRequest("ข้อมูลบัญชีรับโอนไม่ถูกต้อง");
        }

        SetSetting("BankName", bankName?.Trim() ?? string.Empty);
        SetSetting("BankAccount", bankAccount?.Trim() ?? string.Empty);
        SetSetting("BankAccountName", bankAccountName?.Trim() ?? string.Empty);
        _db.SaveChanges();
        TempData["ManagementMessage"] = "บันทึกข้อมูลบัญชีรับโอนแล้ว";
        return RedirectToAction(nameof(Settings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateAddOn(int serviceId, string serviceName, decimal pricePerDay, bool isActive)
    {
        if (!RoleAccess.HasRole(_db, User, "Manager", "Admin"))
        {
            return Forbid();
        }

        var service = _db.AddOnServices.FirstOrDefault(item => item.ServiceId == serviceId);
        if (service is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(serviceName) ||
            serviceName.Length > 150 ||
            pricePerDay < 0 ||
            pricePerDay > 99999999m)
        {
            return BadRequest("ข้อมูลบริการเสริมไม่ถูกต้อง");
        }

        service.ServiceName = serviceName.Trim();
        service.PricePerDay = pricePerDay;
        service.IsActive = isActive;
        _db.SaveChanges();
        TempData["ManagementMessage"] = "บันทึกบริการเสริมแล้ว";
        return RedirectToAction(nameof(AddOnServices));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SavePromotion(
        int? promotionId,
        string promotionName,
        string? promotionCode,
        string conditionType,
        decimal discountPercent,
        string? startDate,
        string? endDate,
        bool isActive = true)
    {
        if (!RoleAccess.HasRole(_db, User, "Manager", "Admin"))
        {
            return Forbid();
        }

        string[] conditions = ["new_user", "3_days", "7_days", "weekday", "festival", "custom"];
        bool isStartDateValid = TryParseFormDate(startDate, out DateTime? parsedStartDate);
        bool isEndDateValid = TryParseFormDate(endDate, out DateTime? parsedEndDate);
        if (string.IsNullOrWhiteSpace(promotionName) ||
            promotionName.Length > 150 ||
            promotionCode?.Length > 50 ||
            !conditions.Contains(conditionType) ||
            discountPercent is <= 0 or > 100 ||
            !isStartDateValid ||
            !isEndDateValid ||
            (parsedStartDate.HasValue && parsedEndDate.HasValue && parsedStartDate > parsedEndDate) ||
            (conditionType == "festival" && (!parsedStartDate.HasValue || !parsedEndDate.HasValue)))
        {
            TempData["ManagementError"] = "ข้อมูลโปรโมชั่นไม่ถูกต้อง";
            return RedirectToAction(nameof(Promotions));
        }

        string? normalizedCode = string.IsNullOrWhiteSpace(promotionCode) ? null : promotionCode.Trim().ToUpperInvariant();
        bool duplicateCode = normalizedCode is not null && _db.Promotions.Any(promotion =>
            promotion.PromotionCode == normalizedCode && promotion.PromotionId != promotionId);
        if (duplicateCode)
        {
            TempData["ManagementError"] = "รหัสโปรโมชั่นนี้ถูกใช้แล้ว";
            return RedirectToAction(nameof(Promotions));
        }

        PromotionModel promotion;
        if (promotionId.HasValue)
        {
            promotion = _db.Promotions.FirstOrDefault(item => item.PromotionId == promotionId.Value)!;
            if (promotion is null)
            {
                return NotFound();
            }
        }
        else
        {
            promotion = new PromotionModel();
            _db.Promotions.Add(promotion);
        }

        promotion.PromotionName = promotionName.Trim();
        promotion.PromotionCode = normalizedCode;
        promotion.ConditionType = conditionType;
        promotion.DiscountPercent = discountPercent;
        promotion.StartDate = parsedStartDate;
        promotion.EndDate = parsedEndDate;
        promotion.IsActive = isActive;
        _db.SaveChanges();
        TempData["ManagementMessage"] = "บันทึกโปรโมชั่นเรียบร้อย";
        return RedirectToAction(nameof(Promotions));
    }

    private static bool TryParseFormDate(string? value, out DateTime? date)
    {
        date = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (!DateTime.TryParseExact(
                value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsedDate))
        {
            return false;
        }

        date = parsedDate.Date;
        return true;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TogglePromotion(int promotionId)
    {
        if (!RoleAccess.HasRole(_db, User, "Manager", "Admin"))
        {
            return Forbid();
        }

        var promotion = _db.Promotions.FirstOrDefault(item => item.PromotionId == promotionId);
        if (promotion is null)
        {
            return NotFound();
        }

        promotion.IsActive = !promotion.IsActive;
        _db.SaveChanges();
        return RedirectToAction(nameof(Promotions));
    }

    [HttpGet]
    public IActionResult Users()
    {
        bool isAdmin = RoleAccess.HasRole(_db, User, "Admin");
        if (!isAdmin && !RoleAccess.HasRole(_db, User, "Manager"))
        {
            return Forbid();
        }

        ViewBag.AvailableRoles = isAdmin
            ? AvailableRoles
            : AvailableRoles.Where(role => role is not ("Admin" or "Manager")).ToArray();
        return View(_db.UserModels.OrderBy(user => user.Username).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateUser(int userId, string role, bool isActive)
    {
        bool isAdmin = RoleAccess.HasRole(_db, User, "Admin");
        if (!isAdmin && !RoleAccess.HasRole(_db, User, "Manager"))
        {
            return Forbid();
        }

        var target = _db.UserModels.FirstOrDefault(item => item.Id == userId);
        if (target is null)
        {
            return NotFound();
        }

        if (!AvailableRoles.Contains(role) ||
            (!isAdmin && (role is "Admin" or "Manager" || target.Role is "Admin" or "Manager")))
        {
            return BadRequest("บทบาทไม่ถูกต้อง");
        }

        var user = target;

        if (user.Username == User.Identity?.Name && (!isActive || role != "Admin"))
        {
            return BadRequest("ไม่สามารถปิดใช้งานหรือลดสิทธิ์บัญชีที่กำลังเข้าสู่ระบบ");
        }

        if (isAdmin && user.Role == "Admin" && (role != "Admin" || !isActive) &&
            _db.UserModels.Count(item => item.Role == "Admin" && item.IsActive) <= 1)
        {
            return BadRequest("ระบบต้องมีบัญชี Admin ที่ใช้งานได้อย่างน้อยหนึ่งบัญชี");
        }

        user.Role = role;
        user.IsActive = isActive;
        _db.SaveChanges();
        TempData["ManagementMessage"] = "อัปเดตสิทธิ์ผู้ใช้งานแล้ว";
        return RedirectToAction(nameof(Users));
    }

    private void SetSetting(string key, string value)
    {
        var setting = _db.SystemSettings.FirstOrDefault(item => item.SettingKey == key);
        if (setting is null)
        {
            _db.SystemSettings.Add(new SystemSettingModel { SettingKey = key, SettingValue = value });
        }
        else
        {
            setting.SettingValue = value;
        }
    }
}
