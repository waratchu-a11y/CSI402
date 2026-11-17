using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        return View(_db.MaintenanceRecords.OrderByDescending(record => record.CreatedAt).ToList());
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
