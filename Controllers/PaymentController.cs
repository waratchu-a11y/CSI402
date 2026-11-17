using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warat_0583_l005.Models;
using warat_0583_l005.Services;

namespace warat_0583_l005.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly Csi402dbContext _db;
    private readonly PrivateUploadStorage _uploadStorage;

    public PaymentController(Csi402dbContext db, PrivateUploadStorage uploadStorage)
    {
        _db = db;
        _uploadStorage = uploadStorage;
    }

    [HttpGet]
    public IActionResult SubmitDeposit(int bookingId)
    {
        if (!RoleAccess.HasRole(_db, User, "Customer"))
        {
            return Forbid();
        }

        var booking = FindOwnedBooking(bookingId);
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.BookingStatus is not ("pending_payment" or "payment_rejected"))
        {
            return Conflict("รายการนี้ไม่สามารถส่งหลักฐานมัดจำได้ในสถานะปัจจุบัน");
        }
        ViewBag.Booking = booking;
        ViewBag.RejectedPayment = _db.Payments
            .Where(payment => payment.BookingId == bookingId &&
                payment.PaymentType == "deposit" &&
                payment.PaymentStatus == "rejected")
            .OrderByDescending(payment => payment.PaymentId)
            .FirstOrDefault();
        SetBankDetails();
        return View(booking);
    }

    [HttpGet]
    public IActionResult SubmitRentalPayment(int bookingId)
    {
        if (!RoleAccess.HasRole(_db, User, "Customer"))
        {
            return Forbid();
        }

        var booking = FindOwnedBooking(bookingId);
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.BookingStatus != "confirmed")
        {
            return Conflict("ชำระค่าเช่าได้หลังจากยืนยันมัดจำแล้วเท่านั้น");
        }
        decimal paidAmount = _db.Payments
            .Where(payment => payment.BookingId == bookingId &&
                payment.PaymentType == "rental" &&
                payment.PaymentStatus == "verified")
            .Sum(payment => (decimal?)payment.Amount) ?? 0m;
        decimal remainingAmount = Math.Max(0m, booking.TotalPrice - paidAmount);
        if (remainingAmount == 0)
        {
            return Conflict("ชำระค่าเช่าครบแล้ว");
        }

        bool pendingExists = _db.Payments.Any(payment =>
            payment.BookingId == bookingId &&
            payment.PaymentType == "rental" &&
            payment.PaymentStatus == "pending");
        if (pendingExists)
        {
            return Conflict("มีหลักฐานค่าเช่าที่กำลังรอตรวจสอบแล้ว");
        }

        ViewBag.AmountDue = remainingAmount;
        SetBankDetails();
        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitRentalPayment(int bookingId, string? referenceNumber, IFormFile? proofFile)
    {
        if (!RoleAccess.HasRole(_db, User, "Customer"))
        {
            return Forbid();
        }

        var booking = FindOwnedBooking(bookingId);
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.BookingStatus != "confirmed")
        {
            return Conflict("ชำระค่าเช่าได้หลังจากยืนยันมัดจำแล้วเท่านั้น");
        }
        if (!IsBankConfigured())
        {
            return Conflict("ยังไม่ได้ตั้งค่าบัญชีรับโอน กรุณาติดต่อผู้ดูแลระบบ");
        }

        bool pendingExists = _db.Payments.Any(payment =>
            payment.BookingId == bookingId &&
            payment.PaymentType == "rental" &&
            payment.PaymentStatus == "pending");
        if (pendingExists)
        {
            return Conflict("มีหลักฐานค่าเช่าที่กำลังรอตรวจสอบแล้ว");
        }

        decimal paidAmount = _db.Payments
            .Where(payment => payment.BookingId == bookingId &&
                payment.PaymentType == "rental" &&
                payment.PaymentStatus == "verified")
            .Sum(payment => (decimal?)payment.Amount) ?? 0m;
        decimal amountDue = Math.Max(0m, booking.TotalPrice - paidAmount);
        if (amountDue <= 0)
        {
            return Conflict("ชำระค่าเช่าครบแล้ว");
        }

        string? proofFileName = await _uploadStorage.SaveImageAsync(proofFile, HttpContext.RequestAborted);
        if (proofFileName is null)
        {
            ModelState.AddModelError(nameof(proofFile), "กรุณาแนบสลิปโอนเงิน JPG หรือ PNG ขนาดไม่เกิน 5 MB");
            ViewBag.AmountDue = amountDue;
            SetBankDetails();
            return View(booking);
        }

        _db.Payments.Add(new PaymentModel
        {
            BookingId = bookingId,
            PaymentType = "rental",
            Amount = amountDue,
            PaymentStatus = "pending",
            ProofFileName = proofFileName,
            ReferenceNumber = string.IsNullOrWhiteSpace(referenceNumber) ? null : referenceNumber.Trim()
        });
        _db.SaveChanges();
        return RedirectToAction(nameof(Submitted), new { bookingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitDeposit(int bookingId, string? referenceNumber, IFormFile? proofFile)
    {
        if (!RoleAccess.HasRole(_db, User, "Customer"))
        {
            return Forbid();
        }

        var booking = FindOwnedBooking(bookingId);
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.BookingStatus is not ("pending_payment" or "payment_rejected"))
        {
            return Conflict("รายการนี้ไม่สามารถส่งหลักฐานมัดจำได้ในสถานะปัจจุบัน");
        }
        if (!IsBankConfigured())
        {
            return Conflict("ยังไม่ได้ตั้งค่าบัญชีรับโอน กรุณาติดต่อผู้ดูแลระบบ");
        }

        bool pendingPaymentExists = _db.Payments.Any(payment =>
            payment.BookingId == bookingId &&
            payment.PaymentType == "deposit" &&
            payment.PaymentStatus == "pending");
        if (pendingPaymentExists)
        {
            return Conflict("มีหลักฐานการชำระเงินที่กำลังรอตรวจสอบแล้ว");
        }

        string? proofFileName = await _uploadStorage.SaveImageAsync(proofFile, HttpContext.RequestAborted);
        if (proofFileName is null)
        {
            ModelState.AddModelError(nameof(proofFile), "กรุณาแนบสลิปโอนเงิน JPG หรือ PNG ขนาดไม่เกิน 5 MB");
            ViewBag.Booking = booking;
            SetBankDetails();
            return View(booking);
        }

        _db.Payments.Add(new PaymentModel
        {
            BookingId = bookingId,
            PaymentType = "deposit",
            Amount = booking.DepositAmount,
            PaymentStatus = "pending",
            ProofFileName = proofFileName,
            ReferenceNumber = string.IsNullOrWhiteSpace(referenceNumber) ? null : referenceNumber.Trim(),
            CreatedAt = DateTime.Now
        });
        booking.BookingStatus = "payment_review";
        _db.SaveChanges();

        return RedirectToAction(nameof(Submitted), new { bookingId });
    }

    [HttpGet]
    public IActionResult Submitted(int bookingId)
    {
        if (!RoleAccess.HasRole(_db, User, "Customer"))
        {
            return Forbid();
        }

        var booking = FindOwnedBooking(bookingId);
        return booking is null ? NotFound() : View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitLicense(int bookingId, IFormFile? licenseImage)
    {
        if (!RoleAccess.HasRole(_db, User, "Customer"))
        {
            return Forbid();
        }

        var booking = FindOwnedBooking(bookingId);
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.BookingStatus != "confirmed" || booking.LicenseStatus != "rejected")
        {
            return Conflict("ยังไม่สามารถส่งใบขับขี่ใหม่สำหรับรายการนี้ได้");
        }

        string? fileName = await _uploadStorage.SaveImageAsync(licenseImage, HttpContext.RequestAborted);
        if (fileName is null)
        {
            TempData["PaymentMessage"] = "ไฟล์ใบขับขี่ไม่ถูกต้อง กรุณาใช้ JPG/PNG ขนาดไม่เกิน 5 MB";
            return RedirectToAction("MyBookings", "Home");
        }

        booking.LicenseImagePath = fileName;
        booking.LicenseStatus = "pending";
        _db.SaveChanges();
        TempData["PaymentMessage"] = "ส่งใบขับขี่ฉบับใหม่แล้ว รอพนักงานตรวจสอบ";
        return RedirectToAction("MyBookings", "Home");
    }

    [HttpGet]
    public IActionResult ReviewQueue()
    {
        if (!RoleAccess.HasRole(_db, User, "Finance", "Admin"))
        {
            return Forbid();
        }

        var payments = _db.Payments
            .Where(payment => payment.PaymentStatus == "pending")
            .OrderBy(payment => payment.CreatedAt)
            .ToList();
        var bookingIds = payments.Select(payment => payment.BookingId).Distinct().ToList();
        ViewBag.Bookings = _db.Bookings.Where(booking => bookingIds.Contains(booking.BookingId)).ToDictionary(booking => booking.BookingId);
        ViewBag.RefundableBookings = _db.Bookings
            .Where(booking => booking.BookingStatus == "completed" &&
                _db.Payments.Any(payment => payment.BookingId == booking.BookingId &&
                    payment.PaymentType == "deposit" && payment.PaymentStatus == "verified") &&
                _db.CarInspections.Any(inspection => inspection.BookingId == booking.BookingId &&
                    inspection.InspectionStage == "after_return") &&
                !_db.Payments.Any(payment => payment.BookingId == booking.BookingId &&
                    (payment.PaymentType == "refund" || payment.PaymentType == "deduction") &&
                    (payment.PaymentStatus == "verified" || payment.PaymentStatus == "completed")))
            .OrderByDescending(booking => booking.BookingId)
            .ToList();
        return View(payments);
    }

    [HttpGet]
    public IActionResult History()
    {
        if (!RoleAccess.HasRole(_db, User, "Finance", "Admin"))
        {
            return Forbid();
        }

        return View(_db.Payments.OrderByDescending(payment => payment.CreatedAt).Take(500).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Review(int paymentId, bool approve, string? note)
    {
        if (!RoleAccess.HasRole(_db, User, "Finance", "Admin"))
        {
            return Forbid();
        }

        var payment = _db.Payments.FirstOrDefault(item => item.PaymentId == paymentId);
        if (payment is null)
        {
            return NotFound();
        }

        if (payment.PaymentStatus != "pending")
        {
            return Conflict("รายการชำระเงินนี้ถูกตรวจสอบแล้ว");
        }

        var booking = _db.Bookings.FirstOrDefault(item => item.BookingId == payment.BookingId);
        if (booking is null)
        {
            return NotFound();
        }

        if (payment.PaymentType == "refund" || booking.BookingStatus == "cancelled")
        {
            return Conflict("รายการคืนเงินจากการยกเลิกต้องบันทึกเลขอ้างอิงหลังโอนคืน หรือรายการจองถูกยกเลิกแล้ว");
        }

        payment.PaymentStatus = approve ? "verified" : "rejected";
        payment.ReviewedBy = User.Identity?.Name;
        payment.ReviewedAt = DateTime.Now;
        payment.Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (payment.PaymentType == "deposit")
        {
            booking.BookingStatus = approve ? "confirmed" : "payment_rejected";
        }
        else if (payment.PaymentType == "deduction" && approve)
        {
            payment.PaymentStatus = "verified";
        }
        _db.SaveChanges();

        return RedirectToAction(nameof(ReviewQueue));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CompleteCancellationRefund(int paymentId, string? transferReference)
    {
        if (!RoleAccess.HasRole(_db, User, "Finance", "Admin"))
        {
            return Forbid();
        }

        using var transaction = _db.Database.BeginTransaction();
        var refund = _db.Payments
            .FromSqlInterpolated($"SELECT * FROM payments WHERE PaymentId = {paymentId} FOR UPDATE")
            .ToList()
            .FirstOrDefault(payment => payment.PaymentType == "refund");
        if (refund is null)
        {
            return NotFound();
        }

        var booking = _db.Bookings.FirstOrDefault(item => item.BookingId == refund.BookingId);
        if (booking is null)
        {
            return NotFound();
        }

        if (refund.PaymentStatus != "pending" || booking.BookingStatus != "cancelled")
        {
            return Conflict("ไม่พบคำขอคืนเงินจากรายการยกเลิกที่รอดำเนินการ");
        }

        if (string.IsNullOrWhiteSpace(transferReference) || transferReference.Length > 100)
        {
            return BadRequest("กรุณาระบุเลขอ้างอิงการโอนคืน");
        }

        decimal verifiedPayments = _db.Payments
            .Where(payment => payment.BookingId == booking.BookingId &&
                (payment.PaymentType == "deposit" || payment.PaymentType == "rental") &&
                payment.PaymentStatus == "verified")
            .Sum(payment => (decimal?)payment.Amount) ?? 0m;
        decimal completedRefunds = _db.Payments
            .Where(payment => payment.BookingId == booking.BookingId &&
                payment.PaymentType == "refund" &&
                payment.PaymentStatus == "completed")
            .Sum(payment => (decimal?)payment.Amount) ?? 0m;
        decimal refundDue = Math.Max(0m, verifiedPayments - completedRefunds);
        if (refundDue <= 0m || refund.Amount != refundDue)
        {
            return Conflict("ยอดคืนเงินเปลี่ยนแปลง กรุณาตรวจสอบประวัติการเงินก่อนดำเนินการ");
        }

        refund.PaymentStatus = "completed";
        refund.ReferenceNumber = transferReference.Trim();
        refund.ReviewedBy = User.Identity?.Name;
        refund.ReviewedAt = DateTime.Now;
        _db.SaveChanges();
        transaction.Commit();
        TempData["PaymentMessage"] = $"บันทึกการคืนเงินจากการยกเลิก Booking #{booking.BookingId} แล้ว";
        return RedirectToAction(nameof(ReviewQueue));
    }

    [HttpGet]
    public IActionResult Proof(int paymentId)
    {
        var payment = _db.Payments.FirstOrDefault(item => item.PaymentId == paymentId);
        if (payment?.ProofFileName is null)
        {
            return NotFound();
        }

        var booking = _db.Bookings.FirstOrDefault(item => item.BookingId == payment.BookingId);
        if (booking is null)
        {
            return NotFound();
        }

        bool isOwner = booking.UserId == User.Identity?.Name;
        bool isFinance = RoleAccess.HasRole(_db, User, "Finance", "Admin");
        if (!isOwner && !isFinance)
        {
            return Forbid();
        }

        string? contentType = PrivateUploadStorage.GetImageContentType(payment.ProofFileName);
        if (contentType is null)
        {
            return NotFound();
        }

        var stream = _uploadStorage.OpenRead(payment.ProofFileName);
        return stream is null ? NotFound() : File(stream, contentType);
    }

    [HttpGet]
    public IActionResult License(int bookingId)
    {
        var booking = _db.Bookings.FirstOrDefault(item => item.BookingId == bookingId);
        if (booking?.LicenseImagePath is null)
        {
            return NotFound();
        }

        bool isOwner = booking.UserId == User.Identity?.Name;
        bool isStaff = RoleAccess.HasRole(_db, User, "Employee", "Service", "Finance", "Manager", "Admin");
        if (!isOwner && !isStaff)
        {
            return Forbid();
        }

        string? contentType = PrivateUploadStorage.GetImageContentType(booking.LicenseImagePath);
        if (contentType is null)
        {
            return NotFound();
        }

        var stream = _uploadStorage.OpenRead(booking.LicenseImagePath);
        return stream is null ? NotFound() : File(stream, contentType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CompleteRefund(int bookingId, string? transferReference)
    {
        if (!RoleAccess.HasRole(_db, User, "Finance", "Admin"))
        {
            return Forbid();
        }

        var booking = _db.Bookings.FirstOrDefault(item => item.BookingId == bookingId);
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.BookingStatus != "completed" ||
            !_db.Payments.Any(payment => payment.BookingId == bookingId &&
                payment.PaymentType == "deposit" &&
                payment.PaymentStatus == "verified"))
        {
            return Conflict("คืนมัดจำได้หลังรถคืนเรียบร้อยและตรวจสอบมัดจำแล้วเท่านั้น");
        }

        if (string.IsNullOrWhiteSpace(transferReference) || transferReference.Length > 100)
        {
            return BadRequest("กรุณาระบุเลขอ้างอิงการโอนคืน");
        }

        var returnInspection = _db.CarInspections
            .Where(inspection => inspection.BookingId == bookingId && inspection.InspectionStage == "after_return")
            .OrderByDescending(inspection => inspection.InspectionId)
            .FirstOrDefault();
        if (returnInspection is null)
        {
            return Conflict("ยังไม่มีผลตรวจสภาพรถหลังคืน");
        }
        var maintenanceItems = _db.MaintenanceRecords
            .Where(item => item.BookingId == bookingId)
            .ToList();
        if (returnInspection.DamageAmount > 0 &&
            (maintenanceItems.Count == 0 || maintenanceItems.Any(item => item.MaintenanceStatus != "completed")))
        {
            return Conflict("ต้องให้ทีมเซอร์วิสสรุปค่าซ่อมและปิดงานก่อนคืนมัดจำ");
        }

        if (_db.Payments.Any(payment =>
            payment.BookingId == bookingId &&
            (payment.PaymentType == "refund" || payment.PaymentType == "deduction") &&
            (payment.PaymentStatus == "verified" || payment.PaymentStatus == "completed")))
        {
            return Conflict("มีการบันทึกการคืนมัดจำหรือหักค่าเสียหายสำหรับรายการนี้แล้ว");
        }

        decimal priorRefundsAndDeductions = _db.Payments
            .Where(payment => payment.BookingId == bookingId &&
                (payment.PaymentType == "refund" || payment.PaymentType == "deduction") &&
                payment.PaymentStatus == "verified")
            .Sum(payment => (decimal?)payment.Amount) ?? 0m;
        decimal remainingDeposit = Math.Max(0m, booking.DepositAmount - priorRefundsAndDeductions);
        decimal damageAmount = maintenanceItems.Count > 0
            ? maintenanceItems.Sum(item => item.RepairCost)
            : returnInspection.DamageAmount;
        decimal damageDeduction = Math.Min(remainingDeposit, damageAmount);
        decimal refundableAmount = Math.Max(0m, remainingDeposit - damageDeduction);

        if (refundableAmount > 0)
        {
            _db.Payments.Add(new PaymentModel
            {
                BookingId = bookingId,
                PaymentType = "refund",
                Amount = refundableAmount,
                PaymentStatus = "completed",
                ReferenceNumber = transferReference.Trim(),
                Note = damageDeduction > 0 ? $"หักค่าซ่อม {damageDeduction:N2} บาท" : "คืนมัดจำเต็มจำนวน",
                ReviewedBy = User.Identity?.Name,
                ReviewedAt = DateTime.Now
            });
        }

        if (damageDeduction > 0)
        {
            _db.Payments.Add(new PaymentModel
            {
                BookingId = bookingId,
                PaymentType = "deduction",
                Amount = damageDeduction,
                PaymentStatus = "verified",
                ReferenceNumber = transferReference.Trim(),
                Note = "หักค่าเสียหายจากเงินมัดจำ",
                ReviewedBy = User.Identity?.Name,
                ReviewedAt = DateTime.Now
            });
        }

        _db.SaveChanges();
        return RedirectToAction(nameof(ReviewQueue));
    }

    private BookingModel? FindOwnedBooking(int bookingId) =>
        _db.Bookings.FirstOrDefault(booking =>
            booking.BookingId == bookingId &&
            booking.UserId == User.Identity!.Name);

    private void SetBankDetails()
    {
        var settings = _db.SystemSettings.ToDictionary(setting => setting.SettingKey, setting => setting.SettingValue);
        ViewBag.BankName = settings.GetValueOrDefault("BankName");
        ViewBag.BankAccount = settings.GetValueOrDefault("BankAccount");
        ViewBag.BankAccountName = settings.GetValueOrDefault("BankAccountName");
    }

    private bool IsBankConfigured() =>
        _db.SystemSettings.Any(setting => setting.SettingKey == "BankAccount" && setting.SettingValue != "");
}
