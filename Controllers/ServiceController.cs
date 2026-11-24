using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warat_0583_l005.Models;
using warat_0583_l005.Services;

namespace warat_0583_l005.Controllers;

[Authorize]
public class ServiceController : Controller
{
    private readonly Csi402dbContext _db;

    public ServiceController(Csi402dbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (!RoleAccess.HasRole(_db, User, "Service", "Admin"))
        {
            return Forbid();
        }

        ViewBag.Cars = _db.Cars.OrderBy(car => car.CarName).ToList();
        ViewBag.Inspections = _db.CarInspections
            .OrderByDescending(inspection => inspection.InspectedAt)
            .Take(100)
            .ToList();
        var returnReviewBookings = _db.Bookings
            .Where(booking => booking.BookingStatus == "service_review" && booking.CarId.HasValue)
            .OrderBy(booking => booking.ReturnSubmittedAt)
            .ToList();
        var returnReviewBookingIds = returnReviewBookings.Select(booking => booking.BookingId).ToList();
        ViewBag.ReturnReceivedInspections = _db.CarInspections
            .Where(inspection => returnReviewBookingIds.Contains(inspection.BookingId) &&
                inspection.InspectionStage == "return_received")
            .ToList()
            .GroupBy(inspection => inspection.BookingId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(inspection => inspection.InspectionId).First());
        ViewBag.ReturnReviewBookings = returnReviewBookings;
        var eligibleBookings = _db.Bookings
            .Where(booking => booking.BookingStatus == "confirmed" &&
                booking.CarId.HasValue &&
                _db.Payments.Any(payment => payment.BookingId == booking.BookingId &&
                    payment.PaymentType == "deposit" &&
                    payment.PaymentStatus == "verified"))
            .OrderBy(booking => booking.PickupDate)
            .ToList();
        var bookingIds = eligibleBookings.Select(booking => booking.BookingId).ToList();
        var latestReleaseInspections = _db.CarInspections
            .Where(inspection => bookingIds.Contains(inspection.BookingId) &&
                inspection.InspectionStage == "pre_release")
            .ToList()
            .GroupBy(inspection => inspection.BookingId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(inspection => inspection.InspectionId).First());
        var releaseInspectionBookings = eligibleBookings
            .Where(booking => !latestReleaseInspections.TryGetValue(booking.BookingId, out var inspection) ||
                inspection.InspectionStatus != "passed")
            .ToList();
        ViewBag.ReleaseInspectionBookings = releaseInspectionBookings;
        var releaseCarIds = releaseInspectionBookings
            .Where(booking => booking.CarId.HasValue)
            .Select(booking => booking.CarId!.Value)
            .Distinct()
            .ToList();
        var carsWithOpenMaintenance = _db.MaintenanceRecords
            .Where(item => releaseCarIds.Contains(item.CarId) && item.MaintenanceStatus != "completed")
            .Select(item => item.CarId)
            .Distinct()
            .ToHashSet();
        var availableCars = _db.Cars
            .Where(car => releaseCarIds.Contains(car.CarId) && car.Status == "ว่าง")
            .Select(car => car.CarId)
            .ToHashSet();
        ViewBag.InspectableReleaseBookingIds = releaseInspectionBookings
            .Where(booking => booking.CarId.HasValue &&
                availableCars.Contains(booking.CarId.Value) &&
                !carsWithOpenMaintenance.Contains(booking.CarId.Value))
            .Select(booking => booking.BookingId)
            .ToHashSet();
        return View(_db.MaintenanceRecords.OrderByDescending(record => record.CreatedAt).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult InspectBeforeRelease(
        int bookingId,
        int mileage,
        int fuelLevel,
        bool readyForRental,
        string? conditionNotes,
        decimal damageAmount)
    {
        if (!RoleAccess.HasRole(_db, User, "Service", "Admin"))
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

        if (booking.BookingStatus != "confirmed" || booking.CarId is null ||
            !_db.Payments.Any(payment => payment.BookingId == bookingId &&
                payment.PaymentType == "deposit" &&
                payment.PaymentStatus == "verified"))
        {
            return Conflict("รถตรวจปล่อยได้หลังยืนยันมัดจำและยืนยันรายการจองแล้วเท่านั้น");
        }

        var latestReleaseInspection = _db.CarInspections
            .Where(inspection => inspection.BookingId == bookingId &&
                inspection.InspectionStage == "pre_release")
            .OrderByDescending(inspection => inspection.InspectionId)
            .FirstOrDefault();
        if (latestReleaseInspection?.InspectionStatus == "passed")
        {
            return Conflict("รายการนี้ผ่านการตรวจปล่อยรถแล้ว");
        }

        var car = _db.Cars.FirstOrDefault(item => item.CarId == booking.CarId.Value);
        if (car is null)
        {
            return NotFound("ไม่พบข้อมูลรถ");
        }

        bool openMaintenance = _db.MaintenanceRecords.Any(item =>
            item.CarId == car.CarId && item.MaintenanceStatus != "completed");
        if (openMaintenance)
        {
            return Conflict("รถยังมีงานซ่อมค้าง ต้องปิดงานซ่อมก่อนจึงตรวจปล่อยซ้ำได้");
        }

        int previousMileage = _db.CarInspections
            .Where(inspection => inspection.CarId == car.CarId)
            .Select(inspection => (int?)inspection.Mileage)
            .Max() ?? 0;
        if (mileage < previousMileage ||
            fuelLevel is < 0 or > 100 ||
            damageAmount is < 0 or > 99999999m ||
            conditionNotes?.Length > 2000 ||
            (readyForRental && damageAmount > 0) ||
            (!readyForRental && string.IsNullOrWhiteSpace(conditionNotes)))
        {
            return BadRequest("ข้อมูลตรวจปล่อยไม่ถูกต้อง ระบุเลขไมล์ น้ำมัน และรายละเอียดปัญหาให้ครบถ้วน");
        }

        _db.CarInspections.Add(new CarInspectionModel
        {
            BookingId = bookingId,
            CarId = car.CarId,
            InspectionStage = "pre_release",
            InspectionStatus = readyForRental ? "passed" : "failed",
            Mileage = mileage,
            FuelLevel = fuelLevel,
            ConditionNotes = string.IsNullOrWhiteSpace(conditionNotes) ? null : conditionNotes.Trim(),
            DamageAmount = damageAmount,
            InspectedBy = User.Identity?.Name ?? string.Empty,
            InspectedAt = DateTime.Now
        });

        if (readyForRental)
        {
            if (car.Status != "ว่าง")
            {
                return Conflict("รถยังไม่พร้อมให้เช่า กรุณาปิดงานซ่อมและอัปเดตสถานะรถก่อน");
            }
        }
        else
        {
            car.Status = "อยู่ระหว่างซ่อม";
            _db.MaintenanceRecords.Add(new MaintenanceModel
            {
                CarId = car.CarId,
                BookingId = bookingId,
                IssueDescription = conditionNotes!.Trim(),
                RepairCost = damageAmount,
                MaintenanceStatus = "reported",
                ReportedBy = User.Identity?.Name ?? string.Empty,
                CreatedAt = DateTime.Now
            });
        }

        _db.SaveChanges();
        transaction.Commit();
        TempData["ServiceMessage"] = readyForRental
            ? $"ตรวจรถก่อนปล่อยเช่า Booking #{bookingId} ผ่านแล้ว"
            : $"ตรวจพบปัญหาใน Booking #{bookingId} ระบบระงับรถและสร้างงานซ่อมแล้ว";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult InspectAfterReturn(
        int bookingId,
        int mileage,
        int fuelLevel,
        bool damageFound,
        decimal damageAmount,
        string conditionNotes)
    {
        if (!RoleAccess.HasRole(_db, User, "Service", "Admin"))
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

        if (booking.BookingStatus != "service_review" ||
            booking.CarId is null ||
            !booking.ReturnSubmittedAt.HasValue)
        {
            return Conflict("รายการนี้ยังไม่ได้แจ้งส่งคืนรถ");
        }

        var pickupInspection = _db.CarInspections
            .Where(inspection => inspection.BookingId == bookingId &&
                inspection.InspectionStage == "before_pickup")
            .OrderByDescending(inspection => inspection.InspectionId)
            .FirstOrDefault();
        var car = _db.Cars.FirstOrDefault(item => item.CarId == booking.CarId.Value);
        if (pickupInspection is null || car is null)
        {
            return Conflict("ไม่พบข้อมูลรถหรือผลตรวจตอนส่งมอบ");
        }

        if (mileage < pickupInspection.Mileage ||
            fuelLevel is < 0 or > 100 ||
            damageAmount is < 0 or > 99999999m ||
            string.IsNullOrWhiteSpace(conditionNotes) ||
            conditionNotes.Length > 2000 ||
            (!damageFound && damageAmount > 0))
        {
            return BadRequest("กรุณาระบุเลขไมล์ น้ำมัน ผลตรวจ และค่าซ่อมให้ถูกต้อง");
        }

        _db.CarInspections.Add(new CarInspectionModel
        {
            BookingId = bookingId,
            CarId = car.CarId,
            InspectionStage = "after_return",
            InspectionStatus = damageFound ? "failed" : "passed",
            Mileage = mileage,
            FuelLevel = fuelLevel,
            ConditionNotes = conditionNotes.Trim(),
            DamageAmount = damageFound ? damageAmount : 0m,
            InspectedBy = User.Identity?.Name ?? string.Empty,
            InspectedAt = DateTime.Now
        });
        booking.BookingStatus = "completed";

        if (damageFound)
        {
            car.Status = "อยู่ระหว่างซ่อม";
            _db.MaintenanceRecords.Add(new MaintenanceModel
            {
                CarId = car.CarId,
                BookingId = bookingId,
                IssueDescription = conditionNotes.Trim(),
                RepairCost = damageAmount,
                MaintenanceStatus = "reported",
                ReportedBy = User.Identity?.Name ?? string.Empty,
                CreatedAt = DateTime.Now
            });
        }
        else
        {
            car.Status = "ว่าง";
        }

        _db.SaveChanges();
        transaction.Commit();
        TempData["ServiceMessage"] = damageFound
            ? $"ตรวจพบความเสียหาย Booking #{bookingId} บันทึกงานซ่อมแล้ว"
            : $"ตรวจรถคืน Booking #{bookingId} แล้ว ไม่พบความเสียหาย";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ReportIssue(int carId, int? bookingId, string issueDescription, decimal repairCost)
    {
        if (!RoleAccess.HasRole(_db, User, "Service", "Admin"))
        {
            return Forbid();
        }

        var car = _db.Cars.FirstOrDefault(item => item.CarId == carId);
        var booking = bookingId.HasValue
            ? _db.Bookings.FirstOrDefault(item => item.BookingId == bookingId && item.CarId == carId)
            : null;
        if (car is null ||
            (bookingId.HasValue && booking is null) ||
            string.IsNullOrWhiteSpace(issueDescription) ||
            issueDescription.Length > 2000 ||
            repairCost < 0 ||
            repairCost > 99999999m)
        {
            return BadRequest("ข้อมูลแจ้งซ่อมไม่ถูกต้อง");
        }

        _db.MaintenanceRecords.Add(new MaintenanceModel
        {
            CarId = carId,
            BookingId = bookingId,
            IssueDescription = issueDescription.Trim(),
            RepairCost = repairCost,
            MaintenanceStatus = "reported",
            ReportedBy = User.Identity?.Name ?? string.Empty,
            CreatedAt = DateTime.Now
        });
        car.Status = "อยู่ระหว่างซ่อม";
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateMaintenance(int maintenanceId, string status, decimal repairCost)
    {
        if (!RoleAccess.HasRole(_db, User, "Service", "Admin"))
        {
            return Forbid();
        }

        var maintenance = _db.MaintenanceRecords.FirstOrDefault(item => item.MaintenanceId == maintenanceId);
        if (maintenance is null)
        {
            return NotFound();
        }

        if (status is not ("reported" or "in_progress" or "completed") ||
            repairCost < 0 || repairCost > 99999999m)
        {
            return BadRequest("สถานะหรือค่าซ่อมไม่ถูกต้อง");
        }

        var car = _db.Cars.FirstOrDefault(item => item.CarId == maintenance.CarId);
        if (car is null)
        {
            return NotFound("ไม่พบข้อมูลรถ");
        }

        maintenance.MaintenanceStatus = status;
        maintenance.RepairCost = repairCost;
        if (status == "completed")
        {
            maintenance.ResolvedBy = User.Identity?.Name;
            maintenance.ResolvedAt = DateTime.Now;
            bool otherOpenWork = _db.MaintenanceRecords.Any(item =>
                item.CarId == car.CarId &&
                item.MaintenanceId != maintenanceId &&
                item.MaintenanceStatus != "completed");
            if (!otherOpenWork)
            {
                car.Status = "ว่าง";
            }
        }
        else
        {
            car.Status = "อยู่ระหว่างซ่อม";
        }

        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
}
