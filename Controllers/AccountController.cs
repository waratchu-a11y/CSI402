using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // เพิ่มสำหรับการจัดการ Cookie
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using warat_0583_l005.Models;
using warat_0583_l005.Services;
using warat_0583_l005.ViewModels;

namespace warat_0583_l005.Controllers
{
    public class AccountController : Controller
    {
        private readonly Csi402dbContext _db;
        private readonly IConfiguration _configuration;
        private readonly PrivateUploadStorage _uploadStorage;
        private readonly PasswordHasher<UserModel> _passwordHasher = new();

        public AccountController(Csi402dbContext db, IConfiguration configuration, PrivateUploadStorage uploadStorage)
        {
            _db = db;
            _configuration = configuration;
            _uploadStorage = uploadStorage;
        }

        // ====================================================
        // 1. หน้า Login (เข้าสู่ระบบ + บันทึกประวัติ + สร้าง Cookie)
        // ====================================================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _db.Set<UserModel>().FirstOrDefault(u => u.Username == model.Username);

                bool passwordMatches = user != null &&
                    !string.IsNullOrWhiteSpace(model.Password) &&
                    !string.IsNullOrEmpty(user.Password) &&
                    user.IsActive &&
                    (user.Password.StartsWith("AQAAAA", StringComparison.Ordinal)
                        ? _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password) != PasswordVerificationResult.Failed
                        : user.Password == model.Password);

                if (user != null && passwordMatches)
                {
                    if (!user.Password!.StartsWith("AQAAAA", StringComparison.Ordinal))
                    {
                        user.Password = _passwordHasher.HashPassword(user, model.Password);
                    }

                    var history = new LoginHistoryModel
                    {
                        UserId = user.Id,
                        Username = user.Username,
                        Email = string.IsNullOrEmpty(user.Email) ? "-" : user.Email,
                        LoggedInAtUtc = DateTime.Now
                    };

                    _db.Add(history);
                    _db.SaveChanges();

                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, user.Username!),
                        new(ClaimTypes.Role, user.Role ?? "Customer"),
                        new(ClaimTypes.NameIdentifier, user.Id.ToString())
                    };
                    var identity = new ClaimsIdentity(claims, "Cookies");
                    await HttpContext.SignInAsync(
                        "Cookies",
                        new ClaimsPrincipal(identity),
                        new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                        });

                    CookieOptions options = new()
                    {
                        Expires = DateTimeOffset.UtcNow.AddHours(8),
                        HttpOnly = true,
                        SameSite = SameSiteMode.Lax,
                        Secure = Request.IsHttps
                    };
                    Response.Cookies.Append("Username", user.Username!, options);

                    // 📌 2. เช็ก Role เพื่อแยกหน้าเว็บ
                    if (user.Role == "Admin")
                    {
                        // ถ้าเป็น Admin ให้ไปหน้า Dashboard ของ Admin
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        // ถ้าเป็น Role อื่นๆ (Customer, Employee, ฯลฯ) ให้ไปหน้าแรกปกติ
                        return RedirectToAction("Indexcustomer", "Home");
                    }
                }

                ViewBag.ErrorMessage = "ชื่อผู้ใช้งานหรือรหัสผ่านไม่ถูกต้อง!";
            }

            return View(model);
        }

        // ====================================================
        // 1.2 ออกจากระบบ (Logout)
        // ====================================================
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            Response.Cookies.Delete("Username");
            Response.Cookies.Delete("Role");
            return RedirectToAction("Indexcustomer", "Home");
        }

        // ====================================================
        // 1.5 หน้าแสดงประวัติการเข้าสู่ระบบ (Login History)
        // ====================================================
        [HttpGet]
        [Authorize]
        public IActionResult LoginHistory()
        {
            if (!RoleAccess.HasRole(_db, User, "Admin", "Manager")) return Forbid();
            var historyList = _db.Set<LoginHistoryModel>()
                                 .OrderByDescending(h => h.LoginHistoryId)
                                 .ToList();
            return View(historyList);
        }

        // ====================================================
        // 2. หน้า Register (สมัครสมาชิก)
        // ====================================================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var checkUser = _db.Set<UserModel>().FirstOrDefault(u => u.Username == model.Username);

                if (checkUser == null)
                {
                    var newUser = new UserModel
                    {
                        Username = model.Username,
                        Password = model.Password,
                        Email = model.Email,
                        Phone = string.IsNullOrEmpty(model.Phone) ? "ไม่ระบุ" : model.Phone,
                        Gender = string.IsNullOrEmpty(model.Gender) ? "ไม่ระบุ" : model.Gender,
                        IdCard = string.IsNullOrEmpty(model.IdCard) ? "ไม่ระบุ" : model.IdCard,
                        Role = "Customer",
                        RegisterDate = DateTime.Now
                    };
                    newUser.Password = _passwordHasher.HashPassword(newUser, model.Password);

                    _db.Add(newUser);
                    _db.SaveChanges();

                    return RedirectToAction("Indexcustomer", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Username นี้มีผู้ใช้งานแล้ว");
                }
            }
            return View(model);
        }

        // ====================================================
        // 3. หน้า Add User (เพิ่มผู้ใช้โดยตรง)
        // ====================================================
        [Authorize]
        [HttpGet]
        public IActionResult AddUser()
        {
            if (!RoleAccess.HasRole(_db, User, "Admin")) return Forbid();
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUser(UserModel user)
        {
            if (!RoleAccess.HasRole(_db, User, "Admin")) return Forbid();
            if (string.IsNullOrWhiteSpace(user.Username) ||
                user.Username.Length > 255 ||
                string.IsNullOrWhiteSpace(user.Password) ||
                user.Password.Length < 8 ||
                string.IsNullOrWhiteSpace(user.Role) ||
                !new[] { "Customer", "Employee", "Manager", "Finance", "Service", "Admin" }.Contains(user.Role) ||
                _db.UserModels.Any(existing => existing.Username == user.Username))
            {
                return BadRequest("ข้อมูลผู้ใช้ไม่ถูกต้องหรือมีชื่อผู้ใช้นี้แล้ว");
            }

            user.RegisterDate = DateTime.Now;
            user.Password = _passwordHasher.HashPassword(user, user.Password);
            _db.Add(user);
            _db.SaveChanges();
            return RedirectToAction("UserList");
        }

        // ====================================================
        // 4. หน้า UserList (แสดงรายชื่อผู้ใช้ทั้งหมด)
        // ====================================================
        [Authorize]
        [HttpGet]
        public IActionResult UserList()
        {
            if (!RoleAccess.HasRole(_db, User, "Admin")) return Forbid();
            var users = _db.Set<UserModel>().ToList();
            return View(users);
        }

        // ====================================================
        // 5. ลบข้อมูลผู้ใช้งาน (Delete)
        // ====================================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!RoleAccess.HasRole(_db, User, "Admin")) return Forbid();
            var user = _db.Set<UserModel>().Find(id);
            if (user != null)
            {
                user.IsActive = false;
                _db.SaveChanges();
            }
            return RedirectToAction("UserList");
        }

        // ====================================================
        // 6. ระบบจองรถ (Lab5 / บันทึกข้อมูลลงฐานข้อมูลจริง)
        // ====================================================
        [HttpGet]
        public IActionResult Lab5()
        {
            return RedirectToAction(nameof(Lab52));
        }

        [HttpPost]
        public IActionResult Lab5(LabUserViewModel data)
        {
            return RedirectToAction(nameof(Lab52));
        }

        // ====================================================
        // 7. หน้า Edit User (แก้ไขข้อมูลผู้ใช้)
        // ====================================================
        [Authorize]
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            if (!RoleAccess.HasRole(_db, User, "Admin")) return Forbid();
            var user = _db.Set<UserModel>().Find(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(UserModel model)
        {
            if (!RoleAccess.HasRole(_db, User, "Admin")) return Forbid();
            var user = _db.UserModels.FirstOrDefault(item => item.Id == model.Id);
            if (user is null) return NotFound();

            if (!string.IsNullOrWhiteSpace(model.Email) &&
                model.Email.Length <= 255 &&
                (string.IsNullOrWhiteSpace(model.Phone) || model.Phone.Length <= 50))
            {
                user.Email = model.Email;
                user.Phone = model.Phone;
                user.Gender = model.Gender;
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    if (model.Password.Length < 8) return BadRequest("รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร");
                    user.Password = _passwordHasher.HashPassword(user, model.Password);
                }
                _db.SaveChanges();
                return RedirectToAction("UserList");
            }
            return View(model);
        }
        
        // ====================================================
        // Lab52 หน้าฟอร์มจองรถแบบ Dynamic จากฐานข้อมูล
        // ====================================================
        [HttpGet]
        public IActionResult Lab52(string? UserId = null, string? CarModel = null, string? PickupLocation = null, string? PickupDate = null, string? ReturnDate = null, int RentalDays = 0, decimal PricePerDay = 0, decimal TotalPrice = 0)
        {
            // ดึงข้อมูลรายการรถยนต์ทั้งหมดส่งไปให้หน้า View (Lab52.cshtml)
            var cars = _db.Set<CarModel>().ToList();
            ViewBag.Cars = cars;
            var selectedCar = cars.FirstOrDefault(car => car.CarName == CarModel);
            string selectedPickupLocation = CarLocationOptions.IsValid(PickupLocation)
                ? PickupLocation!
                : CarLocationOptions.IsValid(selectedCar?.Location)
                    ? selectedCar!.Location!
                    : CarLocationOptions.All[0];
            DateTime pickupDate = TryParseFormDate(PickupDate ?? string.Empty, out DateTime parsedPickupDate)
                ? parsedPickupDate.Date
                : DateTime.Today;
            DateTime returnDate = TryParseFormDate(ReturnDate ?? string.Empty, out DateTime parsedReturnDate)
                ? parsedReturnDate.Date
                : pickupDate.AddDays(RentalDays is >= 1 and <= 30 ? RentalDays : 1);
            if (returnDate <= pickupDate)
            {
                returnDate = pickupDate.AddDays(RentalDays is >= 1 and <= 30 ? RentalDays : 1);
            }

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
            ViewBag.AddonServices = _db.AddOnServices.Where(service => service.IsActive).OrderBy(service => service.ServiceId).ToList();

            ViewBag.UserId = UserId;
            ViewBag.CarModel = CarModel;
            ViewBag.PickupLocation = selectedPickupLocation;
            ViewBag.CarLocations = CarLocationOptions.All;
            ViewBag.PickupDate = PickupDate;
            ViewBag.ReturnDate = ReturnDate;
            ViewBag.RentalDays = RentalDays;
            ViewBag.PricePerDay = PricePerDay;
            ViewBag.TotalPrice = TotalPrice;
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult EligiblePromotions(DateTime pickupDate, int rentalDays)
        {
            bool isAuthenticated = User.Identity?.IsAuthenticated == true;
            if (isAuthenticated && !RoleAccess.HasRole(_db, User, "Customer"))
            {
                return Forbid();
            }

            if (rentalDays is < 1 or > 30 || pickupDate.Date < DateTime.Today)
            {
                return BadRequest("วันรับรถหรือจำนวนวันเช่าไม่ถูกต้อง");
            }

            string? username = isAuthenticated ? User.Identity?.Name : null;
            var promotions = GetEligiblePromotions(username, pickupDate.Date, rentalDays)
                .Select(promotion => new
                {
                    id = promotion.PromotionId,
                    name = promotion.PromotionName,
                    discountPercent = promotion.DiscountPercent,
                    code = promotion.PromotionCode
                })
                .ToList();

            return Json(promotions);
        }

        // ====================================================
        // หน้าใบเสร็จใหม่ (BookingReceipt)
        // ====================================================
        [HttpGet]
        [Authorize]
        public IActionResult BookingReceipt(int id)
        {
            string username = User.Identity!.Name!;
            bool isStaff = RoleAccess.HasRole(_db, User, "Employee", "Service", "Finance", "Manager", "Admin");
            var booking = _db.Bookings.FirstOrDefault(item =>
                item.BookingId == id &&
                (item.UserId == username || isStaff));
            return booking is null ? NotFound() : View(booking);
        }

        // ====================================================
        // 1. หน้าแสดงสรุปข้อมูลเพื่อยืนยันการจอง (GET) - ดึงราคาจาก Database
        // ====================================================
        [Authorize]
        [HttpGet]
        public IActionResult ConfirmBooking(
            string carModel,
            string pickupLocation,
            string returnLocation,
            DateTime pickupDate,
            TimeSpan? pickupTime,
            TimeSpan? returnTime,
            int rentalDays,
            DateTime? returnDate,
            string? promotionCode = null,
            int? promotionId = null,
            bool hasInsurance = false,
            bool hasBabySeat = false)
        {
            if (returnDate.HasValue)
            {
                rentalDays = (returnDate.Value.Date - pickupDate.Date).Days;
            }

            if (string.IsNullOrWhiteSpace(carModel) ||
                !CarLocationOptions.IsValid(pickupLocation) ||
                string.IsNullOrWhiteSpace(returnLocation) ||
                rentalDays is < 1 or > 30 ||
                pickupDate.Date < DateTime.Today ||
                pickupTime is null ||
                returnTime is null ||
                !RoleAccess.HasRole(_db, User, "Customer"))
            {
                return BadRequest("ข้อมูลการจองไม่ถูกต้อง");
            }

            var car = _db.Set<CarModel>().FirstOrDefault(c => c.CarName == carModel);
            if (car is null)
            {
                return NotFound("ไม่พบรถที่เลือก");
            }

            if (!CarLocationOptions.IsValid(car.Location))
            {
                return Conflict("รถคันนี้ยังไม่ได้กำหนดที่ตั้ง กรุณาติดต่อผู้ดูแลระบบ");
            }

            if (!string.Equals(car.Location, pickupLocation, StringComparison.Ordinal))
            {
                return Conflict($"รถคันนี้อยู่ที่ {car.Location} กรุณาเลือกจุดรับรถให้ตรงกับที่ตั้งรถ");
            }

            if (!IsCarAvailable(car, pickupDate.Date, rentalDays))
            {
                return Conflict("รถคันนี้ไม่ว่างในช่วงวันที่เลือก");
            }

            string username = User.Identity!.Name!;
            var promotion = FindEligiblePromotion(username, pickupDate.Date, rentalDays, promotionCode, promotionId);
            if ((!string.IsNullOrWhiteSpace(promotionCode) || promotionId.HasValue) && promotion is null)
            {
                return BadRequest("รหัสโปรโมชั่นไม่ถูกต้องหรือไม่ตรงตามเงื่อนไข");
            }

            if (!TryCalculateAddonTotal(hasInsurance, hasBabySeat, rentalDays, out decimal addonTotal))
            {
                return BadRequest("บริการเสริมที่เลือกไม่พร้อมให้บริการ");
            }
            decimal discountAmount = promotion is null
                ? 0m
                : Math.Round(rentalDays * car.PricePerDay * promotion.DiscountPercent / 100m, 2, MidpointRounding.AwayFromZero);
            decimal totalPrice = rentalDays * car.PricePerDay - discountAmount + addonTotal;
            decimal depositAmount = Math.Round(totalPrice * 0.10m, 2, MidpointRounding.AwayFromZero);

            ViewBag.UserId = username;
            ViewBag.CarModel = car.CarName;
            ViewBag.TransmissionType = car.TransmissionType;
            ViewBag.Seats = car.Seats;
            ViewBag.PickupLocation = pickupLocation;
            ViewBag.ReturnLocation = returnLocation;
            ViewBag.PickupDate = pickupDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            ViewBag.ReturnDate = (returnDate?.Date ?? pickupDate.Date.AddDays(rentalDays))
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            ViewBag.PickupTime = pickupTime.Value.ToString(@"hh\:mm");
            ViewBag.ReturnTime = returnTime.Value.ToString(@"hh\:mm");
            ViewBag.RentalDays = rentalDays;
            ViewBag.PricePerDay = car.PricePerDay;
            ViewBag.HasInsurance = hasInsurance;
            ViewBag.HasBabySeat = hasBabySeat;
            ViewBag.AddonServices = _db.AddOnServices.Where(service => service.IsActive).OrderBy(service => service.ServiceId).ToList();
            ViewBag.AddonTotal = addonTotal;
            ViewBag.DiscountAmount = discountAmount;
            ViewBag.PromotionId = promotion?.PromotionId;
            ViewBag.PromotionCode = promotion?.PromotionCode;
            ViewBag.PromotionName = promotion?.PromotionName;
            ViewBag.TotalPrice = totalPrice;
            ViewBag.DepositAmount = depositAmount;

            return View();
        }

        // ====================================================
        // 2. ฟังก์ชันกดยืนยันการจอง -> บันทึกลง MySQL -> ไปหน้าใบเสร็จ (POST)
        // ====================================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBookingPost(
            string carModel,
            string pickupLocation,
            string returnLocation,
            string pickupDate,
            TimeSpan? pickupTime,
            TimeSpan? returnTime,
            int rentalDays,
            string? returnDate,
            string? promotionCode,
            int? promotionId,
            IFormFile? licenseImage,
            bool hasInsurance = false,
            bool hasBabySeat = false)
        {
            if (!TryParseFormDate(pickupDate, out DateTime parsedPickupDate))
            {
                return BadRequest("วันที่รับรถไม่ถูกต้อง");
            }

            DateTime? parsedReturnDate = null;
            if (!string.IsNullOrWhiteSpace(returnDate))
            {
                if (!TryParseFormDate(returnDate, out DateTime parsedReturn))
                {
                    return BadRequest("วันที่คืนรถไม่ถูกต้อง");
                }

                parsedReturnDate = parsedReturn.Date;
                rentalDays = (parsedReturnDate.Value - parsedPickupDate.Date).Days;
            }

            if (string.IsNullOrWhiteSpace(carModel) ||
                !CarLocationOptions.IsValid(pickupLocation) ||
                string.IsNullOrWhiteSpace(returnLocation) ||
                rentalDays is < 1 or > 30 ||
                parsedPickupDate.Date < DateTime.Today ||
                pickupTime is null ||
                returnTime is null ||
                !RoleAccess.HasRole(_db, User, "Customer"))
            {
                return BadRequest("ข้อมูลการจองไม่ถูกต้อง");
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(HttpContext.RequestAborted);
            var car = _db.Cars
                .FromSqlInterpolated($"SELECT * FROM cars WHERE CarName = {carModel} FOR UPDATE")
                .ToList()
                .FirstOrDefault();
            if (car is null)
            {
                return NotFound("ไม่พบรถที่เลือก");
            }

            if (!CarLocationOptions.IsValid(car.Location))
            {
                return Conflict("รถคันนี้ยังไม่ได้กำหนดที่ตั้ง กรุณาติดต่อผู้ดูแลระบบ");
            }

            if (!string.Equals(car.Location, pickupLocation, StringComparison.Ordinal))
            {
                return Conflict($"รถคันนี้อยู่ที่ {car.Location} กรุณาเลือกจุดรับรถให้ตรงกับที่ตั้งรถ");
            }

            if (!IsCarAvailable(car, parsedPickupDate.Date, rentalDays))
            {
                return Conflict("รถคันนี้ไม่ว่างในช่วงวันที่เลือก");
            }

            string username = User.Identity!.Name!;
            var promotion = FindEligiblePromotion(username, parsedPickupDate.Date, rentalDays, promotionCode, promotionId);
            if ((!string.IsNullOrWhiteSpace(promotionCode) || promotionId.HasValue) && promotion is null)
            {
                return BadRequest("รหัสโปรโมชั่นไม่ถูกต้องหรือไม่ตรงตามเงื่อนไข");
            }

            if (!TryCalculateAddonTotal(hasInsurance, hasBabySeat, rentalDays, out decimal addonTotal))
            {
                return BadRequest("บริการเสริมที่เลือกไม่พร้อมให้บริการ");
            }

            string? licenseImageFileName = await _uploadStorage.SaveImageAsync(licenseImage, HttpContext.RequestAborted);
            if (licenseImageFileName is null)
            {
                ModelState.AddModelError(nameof(licenseImage), "กรุณาแนบรูปใบขับขี่ JPG หรือ PNG ขนาดไม่เกิน 5 MB");
                return BadRequest(ModelState);
            }

            decimal discountAmount = promotion is null
                ? 0m
                : Math.Round(rentalDays * car.PricePerDay * promotion.DiscountPercent / 100m, 2, MidpointRounding.AwayFromZero);
            decimal totalPrice = rentalDays * car.PricePerDay - discountAmount + addonTotal;
            decimal depositAmount = Math.Round(totalPrice * 0.10m, 2, MidpointRounding.AwayFromZero);

            var newBooking = new BookingModel
            {
                UserId = username,
                CarId = car.CarId,
                CarModel = car.CarName,
                PickupLocation = pickupLocation,
                ReturnLocation = returnLocation,
                PickupDate = parsedPickupDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                PickupTime = pickupTime,
                ReturnDate = parsedReturnDate?.Date ?? parsedPickupDate.Date.AddDays(rentalDays),
                ReturnTime = returnTime,
                RentalDays = rentalDays,
                PricePerDay = car.PricePerDay,
                TotalPrice = totalPrice,
                DiscountAmount = discountAmount,
                PromotionCode = promotion?.PromotionCode,
                LicenseImagePath = licenseImageFileName,
                HasInsurance = hasInsurance,
                HasBabySeat = hasBabySeat,
                AddonTotal = addonTotal,
                DepositAmount = depositAmount,
                BookingStatus = "pending_payment",
                BookingDate = DateTime.Now
            };

            _db.Set<BookingModel>().Add(newBooking);
            _db.SaveChanges();
            await transaction.CommitAsync(HttpContext.RequestAborted);

            return RedirectToAction("SubmitDeposit", "Payment", new { bookingId = newBooking.BookingId });
        }

        private bool IsCarAvailable(CarModel car, DateTime pickupDate, int rentalDays)
        {
            if (rentalDays is < 1 or > 30)
            {
                return false;
            }

            DateTime returnDate = pickupDate.AddDays(rentalDays);
            var existingBookings = _db.Set<BookingModel>()
                .Where(booking =>
                    (booking.CarId == car.CarId ||
                     (booking.CarId == null && booking.CarModel == car.CarName)) &&
                    (booking.BookingStatus == "pending" ||
                     booking.BookingStatus == "pending_payment" ||
                     booking.BookingStatus == "payment_review" ||
                     booking.BookingStatus == "confirmed" ||
                     booking.BookingStatus == "rented" ||
                     booking.BookingStatus == "return_requested" ||
                     booking.BookingStatus == "service_review"))
                .ToList();

            return CarAvailability.IsAvailable(car, existingBookings, pickupDate, returnDate);
        }

        private static bool TryParseFormDate(string value, out DateTime date)
        {
            return DateTime.TryParseExact(
                value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        private PromotionModel? FindEligiblePromotion(
            string username,
            DateTime pickupDate,
            int rentalDays,
            string? promotionCode,
            int? promotionId = null)
        {
            var eligiblePromotions = GetEligiblePromotions(username, pickupDate, rentalDays);
            if (promotionId.HasValue)
            {
                return eligiblePromotions.FirstOrDefault(promotion => promotion.PromotionId == promotionId.Value);
            }

            if (!string.IsNullOrWhiteSpace(promotionCode))
            {
                return eligiblePromotions.FirstOrDefault(promotion =>
                    string.Equals(promotion.PromotionCode, promotionCode.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }

        private List<PromotionModel> GetEligiblePromotions(string? username, DateTime pickupDate, int rentalDays)
        {
            DateTime finalRentalDate = pickupDate.AddDays(rentalDays - 1);
            bool isNewCustomer = !string.IsNullOrWhiteSpace(username) &&
                !_db.Set<BookingModel>().Any(booking =>
                    booking.UserId == username &&
                    booking.BookingStatus != "cancelled" &&
                    booking.BookingStatus != "payment_rejected");

            var promotions = _db.Set<PromotionModel>()
                .Where(promotion => promotion.IsActive)
                .ToList()
                .Where(promotion =>
                    (promotion.StartDate == null || promotion.StartDate.Value.Date <= pickupDate) &&
                    (promotion.EndDate == null || promotion.EndDate.Value.Date >= finalRentalDate) &&
                    IsPromotionEligible(promotion, pickupDate, rentalDays, isNewCustomer))
                .OrderByDescending(promotion => promotion.DiscountPercent)
                .ThenBy(promotion => promotion.PromotionId)
                .ToList();

            return promotions;
        }

        private bool TryCalculateAddonTotal(bool hasInsurance, bool hasBabySeat, int rentalDays, out decimal total)
        {
            var servicePrices = _db.AddOnServices
                .Where(service => service.IsActive &&
                    (service.ServiceCode == "INSURANCE" || service.ServiceCode == "BABY_SEAT"))
                .ToDictionary(service => service.ServiceCode, service => service.PricePerDay);

            if ((hasInsurance && !servicePrices.ContainsKey("INSURANCE")) ||
                (hasBabySeat && !servicePrices.ContainsKey("BABY_SEAT")))
            {
                total = 0m;
                return false;
            }

            decimal perDay = (hasInsurance ? servicePrices["INSURANCE"] : 0m) +
                (hasBabySeat ? servicePrices["BABY_SEAT"] : 0m);
            total = perDay * rentalDays;
            return true;
        }

        private static bool IsPromotionEligible(PromotionModel promotion, DateTime pickupDate, int rentalDays, bool isNewCustomer)
        {
            return promotion.ConditionType switch
            {
                "new_user" => isNewCustomer,
                "3_days" => rentalDays >= 3,
                "7_days" => rentalDays >= 7,
                "weekday" => Enumerable.Range(0, rentalDays)
                    .All(offset => pickupDate.AddDays(offset).DayOfWeek is >= DayOfWeek.Monday and <= DayOfWeek.Thursday),
                "festival" => promotion.StartDate.HasValue &&
                    promotion.EndDate.HasValue &&
                    pickupDate.Date >= promotion.StartDate.Value.Date &&
                    pickupDate.Date.AddDays(rentalDays - 1) <= promotion.EndDate.Value.Date,
                "custom" => true,
                _ => false
            };
        }
    }
}