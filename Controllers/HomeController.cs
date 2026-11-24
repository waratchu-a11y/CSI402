using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using warat_0583_l005.Models;
using warat_0583_l005.Services;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace warat_0583_l005.Controllers;

public class HomeController : Controller
{
    private readonly Csi402dbContext _db;

    public HomeController(Csi402dbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var cars = _db.Set<CarModel>()
            .OrderByDescending(car => car.CarId)
            .Take(3)
            .ToList();
        SetUnavailableCars(cars, DateTime.Today, DateTime.Today.AddDays(1));
        ViewBag.CarLocations = CarLocationOptions.All;
        ViewBag.ActivePromotions = GetAdvertisedPromotions();
        return View("Indexcustomer", cars);
    }

    public IActionResult Indexcustomer()
    {
        var cars = _db.Set<CarModel>()
            .OrderByDescending(car => car.CarId)
            .Take(3)
            .ToList();
        SetUnavailableCars(cars, DateTime.Today, DateTime.Today.AddDays(1));
        ViewBag.CarLocations = CarLocationOptions.All;
        ViewBag.ActivePromotions = GetAdvertisedPromotions();
        return View(cars);
    }

    [HttpGet]
    public IActionResult AllCars()
    {
        var cars = _db.Set<CarModel>()
            .OrderByDescending(car => car.CarId)
            .ToList();
        SetUnavailableCars(cars, DateTime.Today, DateTime.Today.AddDays(1));
        ViewBag.CarLocations = CarLocationOptions.All;
        return View(cars);
    }

    public IActionResult Indexadmin()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Lab9()
    {
        return RedirectToAction("Register", "Account");
    }

    [HttpPost]
    public IActionResult Lab9(UserModel data) 
    {
        return RedirectToAction("Register", "Account");
    }

    public IActionResult Lab9_1()
    {
        return RedirectToAction("UserList", "Account");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        if (!RoleAccess.HasRole(_db, User, "Admin"))
        {
            return Forbid();
        }

        var targetUser = _db.Set<UserModel>().Find(id);
        
        if (targetUser != null)
        {
            targetUser.IsActive = false;
            _db.SaveChanges();
        }
        
        return RedirectToAction("UserList", "Account");
    }

    public IActionResult Assessment3()
    {
        string Name, Lastname, Room, Year, Phasa;
        int N1, N2, N3, N4, N5, N6, N7, N8, N9, N10, sum;
        Name = "warat";
        Lastname = "chuenchaichan";
        Room = "L005";
        Year = "ปี3";
        Phasa = "Javascript";

        ViewBag.Name = Name;
        ViewBag.Lastname = Lastname;
        ViewBag.Room = Room;
        ViewBag.Year = Year;
        ViewBag.Phasa = Phasa;

        N1 = 1; N2 = 2; N3 = 3; N4 = 4; N5 = 5; 
        N6 = 6; N7 = 7; N8 = 8; N9 = 9; N10 = 10;
        sum = N1 + N2 + N3 + N4 + N5 + N6 + N7 + N8 + N9 + N10;
        ViewBag.sum = sum;

        if (sum > 80) { ViewBag.Result = "A"; }
        else if (sum >= 76) { ViewBag.Result = "B+"; }
        else if (sum >= 71) { ViewBag.Result = "B"; }
        else if (sum >= 66) { ViewBag.Result = "C+"; }
        else if (sum >= 61) { ViewBag.Result = "C"; }
        else if (sum >= 56) { ViewBag.Result = "D+"; }
        else if (sum >= 51) { ViewBag.Result = "D"; }
        else if (sum < 50) { ViewBag.Result = "F"; }

        return View();
    }

    [HttpGet]
    public IActionResult CarDetail(string carName)
    {
        var car = _db.Set<CarModel>().FirstOrDefault(c => c.CarName == carName);
        
        if (car == null)
        {
            return RedirectToAction("Indexcustomer"); 
        }

        SetUnavailableCars([car], DateTime.Today, DateTime.Today.AddDays(1));
        ViewBag.CarLocations = CarLocationOptions.All;
        ViewBag.CarName = car.CarName; 
        return View(car);
    }

    [HttpGet]
    [Authorize]
    public IActionResult Profile()
    {
        var user = _db.Set<UserModel>().FirstOrDefault(account => account.Username == User.Identity!.Name && account.IsActive);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }
        return View(user);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    [Authorize]
    public IActionResult MyBookings()
    {
        if (!RoleAccess.HasRole(_db, User, "Customer"))
        {
            return Forbid();
        }

        ViewBag.Username = User.Identity?.Name;
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestReturn(int id)
    {
        if (!RoleAccess.HasRole(_db, User, "Customer"))
        {
            return Forbid();
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(HttpContext.RequestAborted);
        var booking = await _db.Bookings
            .FromSqlInterpolated($"SELECT * FROM bookings WHERE BookingId = {id} FOR UPDATE")
            .FirstOrDefaultAsync(HttpContext.RequestAborted);
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.UserId != User.Identity!.Name)
        {
            return Forbid();
        }

        if (booking.BookingStatus != "rented")
        {
            return Conflict("แจ้งคืนรถได้เฉพาะรายการที่กำลังเช่า");
        }

        var car = booking.CarId.HasValue
            ? await _db.Cars.FirstOrDefaultAsync(item => item.CarId == booking.CarId.Value, HttpContext.RequestAborted)
            : null;
        if (car is null)
        {
            return NotFound("ไม่พบข้อมูลรถที่จอง");
        }

        booking.ReturnSubmittedAt = DateTime.Now;
        booking.BookingStatus = "service_review";
        car.Status = "รอตรวจสภาพ";
        await _db.SaveChangesAsync(HttpContext.RequestAborted);
        await transaction.CommitAsync(HttpContext.RequestAborted);
        TempData["PaymentMessage"] = $"แจ้งส่งคืนรถ Booking #{booking.BookingId} แล้ว สถานะ: รอช่างตรวจสอบคืนรถ";
        return RedirectToAction(nameof(MyBookings));
    }

    [HttpGet]
    [Authorize]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult CustomerNotifications()
    {
        if (!RoleAccess.HasRole(_db, User, "Customer"))
        {
            return Forbid();
        }

        var bookings = _db.Bookings
            .Where(booking => booking.UserId == User.Identity!.Name &&
                (booking.BookingStatus == "pending_payment" ||
                 booking.BookingStatus == "payment_rejected" ||
                 booking.BookingStatus == "payment_review" ||
                 booking.BookingStatus == "confirmed"))
            .ToList();
        var bookingIds = bookings.Select(booking => booking.BookingId).ToList();
        var paymentsByBooking = _db.Payments
            .Where(payment => bookingIds.Contains(payment.BookingId))
            .ToList()
            .ToLookup(payment => payment.BookingId);

        var notifications = new List<object>();
        int count = 0;
        foreach (var booking in bookings)
        {
            var payments = paymentsByBooking[booking.BookingId];
            string type;
            string message;
            decimal amount = 0m;

            if (booking.BookingStatus == "pending_payment")
            {
                type = "deposit-needed";
                amount = booking.DepositAmount;
                message = $"กรุณาชำระเงินมัดจำ ฿{amount:N2} สำหรับการจอง #{booking.BookingId}";
            }
            else if (booking.BookingStatus == "payment_rejected")
            {
                type = "deposit-rejected";
                var rejectedDeposit = payments
                    .Where(payment => payment.PaymentType == "deposit" && payment.PaymentStatus == "rejected")
                    .OrderByDescending(payment => payment.PaymentId)
                    .FirstOrDefault();
                message = $"ฝ่ายการเงินปฏิเสธสลิปมัดจำของการจอง #{booking.BookingId}" +
                    (string.IsNullOrWhiteSpace(rejectedDeposit?.Note) ? " กรุณาตรวจสอบและส่งหลักฐานใหม่" : $": {rejectedDeposit.Note}");
            }
            else if (booking.BookingStatus == "payment_review")
            {
                type = "deposit-review";
                message = $"ได้รับสลิปมัดจำของการจอง #{booking.BookingId} แล้ว กำลังตรวจสอบโดยฝ่ายการเงิน";
            }
            else
            {
                var pendingRental = payments
                    .Where(payment => payment.PaymentType == "rental" && payment.PaymentStatus == "pending")
                    .OrderByDescending(payment => payment.PaymentId)
                    .FirstOrDefault();
                var rejectedRental = payments
                    .Where(payment => payment.PaymentType == "rental" && payment.PaymentStatus == "rejected")
                    .OrderByDescending(payment => payment.PaymentId)
                    .FirstOrDefault();
                decimal rentalPaid = payments
                    .Where(payment => payment.PaymentType == "rental" && payment.PaymentStatus == "verified")
                    .Sum(payment => payment.Amount);
                amount = Math.Max(0m, booking.TotalPrice - rentalPaid);

                if (pendingRental is not null)
                {
                    type = $"rental-review-{pendingRental.PaymentId}";
                    message = $"ได้รับสลิปค่าเช่าของการจอง #{booking.BookingId} แล้ว กำลังตรวจสอบโดยฝ่ายการเงิน";
                }
                else if (rejectedRental is not null)
                {
                    type = $"rental-rejected-{rejectedRental.PaymentId}";
                    message = $"ฝ่ายการเงินปฏิเสธสลิปค่าเช่าของการจอง #{booking.BookingId}" +
                        (string.IsNullOrWhiteSpace(rejectedRental.Note) ? " กรุณาตรวจสอบและส่งหลักฐานใหม่" : $": {rejectedRental.Note}");
                }
                else if (amount > 0m)
                {
                    type = "rental-payment-needed";
                    message = $"ฝ่ายการเงินยืนยันมัดจำแล้ว กรุณาชำระค่าเช่า ฿{amount:N2} (ไม่รวมเงินมัดจำ) สำหรับการจอง #{booking.BookingId}";
                }
                else
                {
                    type = "rental-paid";
                    message = $"ฝ่ายการเงินยืนยันการชำระค่าเช่าครบแล้ว สำหรับการจอง #{booking.BookingId}";
                }
            }

            if (type != "rental-paid")
            {
                count++;
            }

            notifications.Add(new
            {
                id = $"{booking.BookingId}:{type}",
                bookingId = booking.BookingId,
                type,
                message,
                amount,
                url = Url.Action(nameof(MyBookings), "Home")
            });
        }

        return Json(new { count, notifications });
    }

    [HttpGet]
    public IActionResult BookingReceipt(string carModel, string pickupLocation, string pickupDate, int rentalDays, decimal pricePerDay, decimal totalPrice)
    {
        ViewBag.CarModel = carModel;
        ViewBag.PickupLocation = pickupLocation;
        ViewBag.PickupDate = pickupDate;
        ViewBag.RentalDays = rentalDays;
        ViewBag.PricePerDay = pricePerDay;
        ViewBag.TotalPrice = totalPrice;
        return View();
    }

    // 📌 ฟังก์ชันสำหรับยกเลิกการจอง
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CancelBooking(int id)
    {
        if (!RoleAccess.HasRole(_db, User, "Customer"))
        {
            return Forbid();
        }

        var booking = _db.Set<BookingModel>()
            .FirstOrDefault(b => b.BookingId == id && b.UserId == User.Identity!.Name);
        
        if (booking != null)
        {
            if (booking.BookingStatus is "pending" or "pending_payment" or "payment_rejected")
            {
                booking.BookingStatus = "cancelled";
            }
            _db.SaveChanges();
        }

        return RedirectToAction("MyBookings");
    }
    // ====================================================
    // 📌 ฟังก์ชันค้นหารถจากหน้าแรก
    // ====================================================
    [HttpGet]
    public IActionResult SearchCars(string? pickupLocation, string? pickupDate, string? returnDate)
    {
        ViewBag.SearchPerformed = true;
        ViewBag.SearchLocation = pickupLocation;
        ViewBag.SearchPickupDate = pickupDate;
        ViewBag.SearchReturnDate = returnDate;
        ViewBag.CarLocations = CarLocationOptions.All;

        if (!CarLocationOptions.IsValid(pickupLocation))
        {
            ViewBag.SearchError = "กรุณาเลือกสถานที่รับรถที่ถูกต้อง";
            return View("AllCars", new List<CarModel>());
        }

        if (!CarAvailability.TryParseDate(pickupDate, out DateTime parsedPickupDate) ||
            !CarAvailability.TryParseDate(returnDate, out DateTime parsedReturnDate) ||
            parsedPickupDate.Date < DateTime.Today ||
            parsedReturnDate.Date <= parsedPickupDate.Date ||
            (parsedReturnDate.Date - parsedPickupDate.Date).Days > 30)
        {
            ViewBag.SearchError = "กรุณาเลือกวันที่รับรถและคืนรถให้ถูกต้อง โดยเช่าได้ไม่เกิน 30 วัน";
            return View("AllCars", new List<CarModel>());
        }

        pickupDate = parsedPickupDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        returnDate = parsedReturnDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        int rentalDays = (parsedReturnDate.Date - parsedPickupDate.Date).Days;
        ViewBag.SearchPickupDate = pickupDate;
        ViewBag.SearchReturnDate = returnDate;
        ViewBag.SearchRentalDays = rentalDays;

        var cars = _db.Set<CarModel>()
            .Where(car => car.Location == pickupLocation)
            .ToList()
            .OrderByDescending(car => car.CarId)
            .ToList();
        SetUnavailableCars(cars, parsedPickupDate, parsedReturnDate);

        return View("AllCars", cars);
    }

    private void SetUnavailableCars(IEnumerable<CarModel> cars, DateTime pickupDate, DateTime returnDate)
    {
        var activeBookings = _db.Bookings
            .Where(booking =>
                booking.BookingStatus == "pending" ||
                booking.BookingStatus == "pending_payment" ||
                booking.BookingStatus == "payment_review" ||
                booking.BookingStatus == "confirmed" ||
                booking.BookingStatus == "rented" ||
                booking.BookingStatus == "return_requested" ||
                booking.BookingStatus == "service_review")
            .ToList();

        ViewBag.UnavailableCarIds = cars
            .Where(car => !CarAvailability.IsAvailable(car, activeBookings, pickupDate, returnDate))
            .Select(car => car.CarId)
            .ToHashSet();
    }

    private List<PromotionModel> GetAdvertisedPromotions()
    {
        DateTime today = DateTime.Today;
        return _db.Promotions
            .Where(promotion => promotion.IsActive &&
                (!promotion.EndDate.HasValue || promotion.EndDate.Value >= today))
            .OrderByDescending(promotion => promotion.DiscountPercent)
            .ThenBy(promotion => promotion.StartDate)
            .Take(6)
            .ToList();
    }
}