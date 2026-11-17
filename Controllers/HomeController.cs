using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
        ViewBag.ActivePromotions = GetAdvertisedPromotions();
        return View("Indexcustomer", cars);
    }

    public IActionResult Indexcustomer()
    {
        var cars = _db.Set<CarModel>()
            .OrderByDescending(car => car.CarId)
            .Take(3)
            .ToList();
        ViewBag.ActivePromotions = GetAdvertisedPromotions();
        return View(cars);
    }

    [HttpGet]
    public IActionResult AllCars()
    {
        var cars = _db.Set<CarModel>()
            .OrderByDescending(car => car.CarId)
            .ToList();
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

        var activeBookings = _db.Bookings
            .Where(booking =>
                booking.BookingStatus == "pending" ||
                booking.BookingStatus == "pending_payment" ||
                booking.BookingStatus == "payment_review" ||
                booking.BookingStatus == "confirmed" ||
                booking.BookingStatus == "rented")
            .ToList();

        var searchResults = _db.Set<CarModel>()
            .ToList()
            .Where(car => CarAvailability.IsAvailable(
                car,
                activeBookings,
                parsedPickupDate,
                parsedReturnDate))
            .OrderByDescending(car => car.CarId)
            .ToList();

        return View("AllCars", searchResults);
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