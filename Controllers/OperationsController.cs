using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warat_0583_l005.Models;
using warat_0583_l005.Services;

namespace warat_0583_l005.Controllers;

[Authorize]
public class OperationsController : Controller
{
    private readonly Csi402dbContext _db;

    public OperationsController(Csi402dbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (!RoleAccess.HasRole(_db, User, "Employee", "Admin"))
        {
            return Forbid();
        }

        var bookings = _db.Bookings
            .Where(booking =>
                booking.BookingStatus == "confirmed" ||
                booking.BookingStatus == "rented" ||
                booking.BookingStatus == "return_requested")
            .OrderBy(booking => booking.PickupDate)
            .ToList();
        ViewBag.LatestInspections = _db.CarInspections
            .ToList()
            .GroupBy(inspection => inspection.BookingId)
            .Select(group => group.OrderByDescending(inspection => inspection.InspectionId).First())
            .ToDictionary(inspection => inspection.BookingId);
        var confirmedBookingIds = bookings
            .Where(booking => booking.BookingStatus == "confirmed" && booking.CarId.HasValue)
            .Select(booking => booking.BookingId)
            .ToList();
        var passedReleaseBookingIds = _db.CarInspections
            .Where(inspection => confirmedBookingIds.Contains(inspection.BookingId) &&
                inspection.InspectionStage == "pre_release")
            .ToList()
            .GroupBy(inspection => inspection.BookingId)
            .Where(group => group.OrderByDescending(inspection => inspection.InspectionId).First().InspectionStatus == "passed")
            .Select(group => group.Key)
            .ToHashSet();
        var carIds = bookings.Where(booking => booking.CarId.HasValue)
            .Select(booking => booking.CarId!.Value)
            .Distinct()
            .ToList();
        var availableCarIds = _db.Cars
            .Where(car => carIds.Contains(car.CarId) && car.Status == "ว่าง")
            .Select(car => car.CarId)
            .ToHashSet();
        var carsWithOpenMaintenance = _db.MaintenanceRecords
            .Where(record => carIds.Contains(record.CarId) && record.MaintenanceStatus != "completed")
            .Select(record => record.CarId)
            .Distinct()
            .ToHashSet();
        ViewBag.ReadyForPickupBookingIds = bookings
            .Where(booking => booking.CarId.HasValue &&
                passedReleaseBookingIds.Contains(booking.BookingId) &&
                availableCarIds.Contains(booking.CarId.Value) &&
                !carsWithOpenMaintenance.Contains(booking.CarId.Value))
            .Select(booking => booking.BookingId)
            .ToHashSet();
        return View(bookings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ReviewLicense(int bookingId, bool approve, string? note)
    {
        if (!RoleAccess.HasRole(_db, User, "Employee", "Admin"))
        {
            return Forbid();
        }

        var booking = _db.Bookings.FirstOrDefault(item => item.BookingId == bookingId);
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.BookingStatus != "confirmed" || booking.LicenseImagePath is null)
        {
            return Conflict("ไม่พบใบขับขี่หรือรายการจองยังไม่พร้อมตรวจสอบ");
        }

        booking.LicenseStatus = approve ? "approved" : "rejected";
        booking.LicenseReviewNote = approve || string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        _db.SaveChanges();
        TempData["OperationsMessage"] = approve ? "ตรวจสอบใบขับขี่เรียบร้อย" : "ปฏิเสธใบขับขี่แล้ว";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Pickup(int bookingId, int mileage, int fuelLevel, string? conditionNotes)
    {
        if (!RoleAccess.HasRole(_db, User, "Employee", "Admin"))
        {
            return Forbid();
        }

        using var transaction = _db.Database.BeginTransaction();
        var booking = _db.Bookings
            .FromSqlInterpolated($"SELECT * FROM bookings WHERE BookingId = {bookingId} FOR UPDATE")
            .ToList()
            .FirstOrDefault();
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.BookingStatus != "confirmed" || booking.LicenseStatus != "approved" || booking.CarId is null)
        {
            return Conflict("ต้องยืนยันมัดจำและตรวจใบขับขี่ก่อนส่งมอบรถ");
        }

        bool depositVerified = _db.Payments.Any(payment =>
            payment.BookingId == bookingId &&
            payment.PaymentType == "deposit" &&
            payment.PaymentStatus == "verified");
        var releaseInspection = _db.CarInspections
            .Where(inspection => inspection.BookingId == bookingId &&
                inspection.InspectionStage == "pre_release")
            .OrderByDescending(inspection => inspection.InspectionId)
            .FirstOrDefault();
        var car = _db.Cars.FirstOrDefault(item => item.CarId == booking.CarId.Value);
        bool openMaintenance = _db.MaintenanceRecords.Any(item =>
            item.CarId == booking.CarId.Value && item.MaintenanceStatus != "completed");
        decimal rentalPaid = _db.Payments
            .Where(payment => payment.BookingId == bookingId &&
                payment.PaymentType == "rental" &&
                payment.PaymentStatus == "verified")
            .Sum(payment => (decimal?)payment.Amount) ?? 0m;
        int previousMileage = _db.CarInspections
            .Where(inspection => inspection.CarId == booking.CarId)
            .Select(inspection => (int?)inspection.Mileage)
            .Max() ?? 0;
        if (!depositVerified || releaseInspection?.InspectionStatus != "passed" ||
            car?.Status != "ว่าง" || openMaintenance ||
            rentalPaid < booking.TotalPrice ||
            mileage < previousMileage || fuelLevel is < 0 or > 100 ||
            conditionNotes?.Length > 2000)
        {
            return BadRequest("ส่งมอบไม่ได้ ต้องให้ทีมเซอร์วิสตรวจรถผ่าน รถต้องพร้อมใช้งาน และยืนยันมัดจำ/ค่าเช่าครบก่อน พร้อมตรวจเลขไมล์และระดับน้ำมัน");
        }

        _db.CarInspections.Add(new CarInspectionModel
        {
            BookingId = bookingId,
            CarId = booking.CarId.Value,
            InspectionStage = "before_pickup",
            Mileage = mileage,
            FuelLevel = fuelLevel,
            ConditionNotes = conditionNotes?.Trim(),
            InspectedBy = User.Identity?.Name ?? string.Empty,
            InspectedAt = DateTime.Now
        });
        booking.BookingStatus = "rented";
        _db.SaveChanges();
        transaction.Commit();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ReceiveReturn(int bookingId, int mileage, int fuelLevel, string? conditionNotes)
    {
        if (!RoleAccess.HasRole(_db, User, "Employee", "Admin"))
        {
            return Forbid();
        }

        using var transaction = _db.Database.BeginTransaction();
        var booking = _db.Bookings
            .FromSqlInterpolated($"SELECT * FROM bookings WHERE BookingId = {bookingId} FOR UPDATE")
            .ToList()
            .FirstOrDefault();
        if (booking is null)
        {
            return NotFound();
        }

        var pickupInspection = _db.CarInspections
            .Where(inspection => inspection.BookingId == bookingId && inspection.InspectionStage == "before_pickup")
            .OrderByDescending(inspection => inspection.InspectionId)
            .FirstOrDefault();
        if (booking.BookingStatus != "return_requested" ||
            booking.CarId is null ||
            pickupInspection is null ||
            !booking.ReturnMileage.HasValue ||
            string.IsNullOrWhiteSpace(booking.ReturnImagePaths))
        {
            return Conflict("ต้องให้ลูกค้าส่งรูปและเลขไมล์ก่อนจึงรับคืนรถได้");
        }

        if (mileage < booking.ReturnMileage.Value ||
            mileage < pickupInspection.Mileage ||
            fuelLevel is < 0 or > 100 ||
            conditionNotes?.Length > 2000)
        {
            return BadRequest("ข้อมูลรับคืนไม่ถูกต้อง เลขไมล์ที่พนักงานตรวจต้องไม่น้อยกว่าที่ลูกค้าแจ้งและเลขไมล์ตอนส่งมอบ");
        }

        var car = _db.Cars.FirstOrDefault(item => item.CarId == booking.CarId);
        if (car is null)
        {
            return NotFound("ไม่พบข้อมูลรถ");
        }

        _db.CarInspections.Add(new CarInspectionModel
        {
            BookingId = bookingId,
            CarId = car.CarId,
            InspectionStage = "return_received",
            Mileage = mileage,
            FuelLevel = fuelLevel,
            ConditionNotes = conditionNotes?.Trim(),
            InspectedBy = User.Identity?.Name ?? string.Empty,
            InspectedAt = DateTime.Now
        });
        booking.BookingStatus = "service_review";
        car.Status = "รอตรวจสภาพ";

        _db.SaveChanges();
        transaction.Commit();
        TempData["OperationsMessage"] = "บันทึกรับรถแล้ว ส่งต่อให้ทีมเซอร์วิสตรวจความเสียหาย";
        return RedirectToAction(nameof(Index));
    }
}
