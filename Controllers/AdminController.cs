using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using warat_0583_l005.Models;
using warat_0583_l005.Services;

namespace warat_0583_l005.Controllers
{
    public class AdminController : Controller
    {
        private readonly Csi402dbContext _db;

        public AdminController(Csi402dbContext db)
        {
            _db = db;
        }

        // 📌 เช็กสิทธิ์แอดมินก่อนเข้าใช้งาน
        private bool IsAdmin()
        {
            return RoleAccess.HasRole(_db, User, "Admin", "Manager");
        }

        // ====================================================
        // 1. หน้า Dashboard หลักของ Admin
        // ====================================================
        [HttpGet]
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.TotalUsers = _db.Set<UserModel>().Count();
            ViewBag.TotalBookings = _db.Set<BookingModel>().Count();
            ViewBag.TotalCars = _db.Set<CarModel>().Count();
            ViewBag.AdminName = User.Identity?.Name ?? "Admin";

            return View();
        }

        // ====================================================
        // 2. หน้ารายการรถยนต์ทั้งหมด (Car Management)
        // ====================================================
        [HttpGet]
        public IActionResult CarManagement()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var cars = _db.Set<CarModel>().ToList();
            return View(cars);
        }

        [HttpGet]
        public IActionResult Bookings(string? userId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var customers = _db.UserModels
                .Where(user => user.Role == "Customer")
                .OrderBy(user => user.Username)
                .ToList();
            ViewBag.Users = customers;
            ViewBag.SelectedUserId = userId;
            ViewBag.CustomerNames = customers
                .GroupBy(user => user.Username ?? string.Empty)
                .ToDictionary(
                    group => group.Key,
                    group => group.First().Username ?? group.First().Email ?? "ไม่ทราบชื่อ");

            var bookingsQuery = _db.Bookings.AsQueryable();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                bookingsQuery = bookingsQuery.Where(booking => booking.UserId == userId);
            }

            var bookings = bookingsQuery
                .OrderByDescending(booking => booking.BookingDate)
                .ThenByDescending(booking => booking.BookingId)
                .ToList();
            var bookingIds = bookings.Select(booking => booking.BookingId).ToList();
            var payments = _db.Payments
                .Where(payment => bookingIds.Contains(payment.BookingId) &&
                    payment.PaymentStatus == "verified" &&
                    (payment.PaymentType == "deposit" || payment.PaymentType == "rental"))
                .ToList()
                .GroupBy(payment => payment.BookingId)
                .ToDictionary(group => group.Key, group => group.Sum(payment => payment.Amount));

            ViewBag.VerifiedPayments = payments;
            ViewBag.EditableBookingIds = bookings
                .Where(CanEditBooking)
                .Select(booking => booking.BookingId)
                .ToHashSet();
            ViewBag.CancellableBookingIds = bookings
                .Where(CanCancelBooking)
                .Select(booking => booking.BookingId)
                .ToHashSet();
            return View(bookings);
        }

        [HttpGet]
        public IActionResult EditBooking(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var booking = _db.Bookings.FirstOrDefault(item => item.BookingId == id);
            if (booking is null)
            {
                return NotFound();
            }

            if (!CanEditBooking(booking))
            {
                TempData["AdminBookingError"] = "แก้ไขได้เฉพาะรายการที่ยังไม่ชำระหรือยังไม่มีรายการชำระที่กำลังตรวจสอบ";
                return RedirectToAction(nameof(Bookings), new { userId = booking.UserId });
            }

            ViewBag.Cars = _db.Cars.OrderBy(car => car.CarName).ToList();
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditBooking(
            int id,
            int carId,
            string pickupDate,
            string returnDate,
            string pickupLocation,
            string returnLocation,
            TimeSpan? pickupTime,
            TimeSpan? returnTime)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var booking = _db.Bookings.FirstOrDefault(item => item.BookingId == id);
            if (booking is null)
            {
                return NotFound();
            }

            if (!CanEditBooking(booking))
            {
                TempData["AdminBookingError"] = "รายการนี้มีการชำระเงินแล้วหรือกำลังดำเนินการ จึงไม่สามารถแก้ไขได้";
                return RedirectToAction(nameof(Bookings), new { userId = booking.UserId });
            }

            if (!DateTime.TryParseExact(pickupDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedPickup) ||
                !DateTime.TryParseExact(returnDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedReturn) ||
                parsedPickup.Date < DateTime.Today ||
                parsedReturn.Date <= parsedPickup.Date ||
                (parsedReturn.Date - parsedPickup.Date).Days > 30 ||
                string.IsNullOrWhiteSpace(pickupLocation) ||
                pickupLocation.Trim().Length > 150 ||
                string.IsNullOrWhiteSpace(returnLocation) ||
                returnLocation.Trim().Length > 150 ||
                pickupTime is null ||
                returnTime is null)
            {
                TempData["AdminBookingError"] = "ข้อมูลวัน เวลา หรือจุดรับ–คืนรถไม่ถูกต้อง";
                return RedirectToAction(nameof(EditBooking), new { id });
            }

            using var transaction = _db.Database.BeginTransaction();
            var car = _db.Cars
                .FromSqlInterpolated($"SELECT * FROM cars WHERE CarId = {carId} FOR UPDATE")
                .ToList()
                .FirstOrDefault();
            if (car is null)
            {
                TempData["AdminBookingError"] = "ไม่พบรถที่เลือก";
                return RedirectToAction(nameof(EditBooking), new { id });
            }

            if (car.Status == "อยู่ระหว่างซ่อม" ||
                (car.Status != "ว่าง" && car.CarId != booking.CarId))
            {
                TempData["AdminBookingError"] = "รถที่เลือกไม่พร้อมให้จอง";
                return RedirectToAction(nameof(EditBooking), new { id });
            }

            var activeBookings = _db.Bookings
                .Where(item => item.BookingId != id &&
                    (item.BookingStatus == "pending" ||
                     item.BookingStatus == "pending_payment" ||
                     item.BookingStatus == "payment_review" ||
                     item.BookingStatus == "confirmed" ||
                     item.BookingStatus == "rented"))
                .ToList();
            var availabilityCar = new CarModel
            {
                CarId = car.CarId,
                CarName = car.CarName,
                Status = "ว่าง"
            };
            if (!CarAvailability.IsAvailable(availabilityCar, activeBookings, parsedPickup.Date, parsedReturn.Date))
            {
                TempData["AdminBookingError"] = "รถคันนี้มีรายการจองทับช่วงวันที่เลือก";
                return RedirectToAction(nameof(EditBooking), new { id });
            }

            int rentalDays = (parsedReturn.Date - parsedPickup.Date).Days;
            bool priceNeedsRecalculation = car.CarId != booking.CarId ||
                !CarAvailability.TryParseDate(booking.PickupDate, out var oldPickup) ||
                oldPickup.Date != parsedPickup.Date ||
                booking.ReturnDate?.Date != parsedReturn.Date;

            decimal discountAmount = booking.DiscountAmount;
            decimal addonTotal = booking.AddonTotal;
            decimal totalPrice = booking.TotalPrice;
            if (priceNeedsRecalculation &&
                !TryCalculateUpdatedPrice(booking, car, parsedPickup.Date, rentalDays, out discountAmount, out addonTotal, out totalPrice))
            {
                TempData["AdminBookingError"] = "ไม่สามารถคำนวณราคาใหม่ได้ กรุณาตรวจสอบเงื่อนไขโปรโมชั่นหรือบริการเสริมของรายการนี้";
                return RedirectToAction(nameof(EditBooking), new { id });
            }

            booking.CarId = car.CarId;
            booking.CarModel = car.CarName;
            booking.PickupDate = parsedPickup.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            booking.ReturnDate = parsedReturn.Date;
            booking.RentalDays = rentalDays;
            booking.PickupLocation = pickupLocation.Trim();
            booking.ReturnLocation = returnLocation.Trim();
            booking.PickupTime = pickupTime;
            booking.ReturnTime = returnTime;

            if (priceNeedsRecalculation)
            {
                booking.PricePerDay = car.PricePerDay;
                booking.DiscountAmount = discountAmount;
                booking.AddonTotal = addonTotal;
                booking.TotalPrice = totalPrice;
                booking.DepositAmount = Math.Round(totalPrice * 0.10m, 2, MidpointRounding.AwayFromZero);
            }

            _db.SaveChanges();
            transaction.Commit();
            TempData["AdminBookingMessage"] = "บันทึกการแก้ไขรายการจองแล้ว";
            return RedirectToAction(nameof(Bookings), new { userId = booking.UserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelBooking(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            using var transaction = _db.Database.BeginTransaction();
            var booking = _db.Bookings
                .FromSqlInterpolated($"SELECT * FROM bookings WHERE BookingId = {id} FOR UPDATE")
                .ToList()
                .FirstOrDefault();
            if (booking is null)
            {
                return NotFound();
            }

            if (!CanCancelBooking(booking))
            {
                TempData["AdminBookingError"] = "ยกเลิกไม่ได้: รถอยู่ระหว่างเช่าหรือรายการคืนรถเสร็จแล้ว";
                return RedirectToAction(nameof(Bookings), new { userId = booking.UserId });
            }

            decimal verifiedPayments = _db.Payments
                .Where(item => item.BookingId == id &&
                    (item.PaymentType == "deposit" || item.PaymentType == "rental") &&
                    item.PaymentStatus == "verified")
                .Sum(item => (decimal?)item.Amount) ?? 0m;
            decimal completedRefunds = _db.Payments
                .Where(item => item.BookingId == id &&
                    item.PaymentType == "refund" &&
                    item.PaymentStatus == "completed")
                .Sum(item => (decimal?)item.Amount) ?? 0m;
            decimal refundDue = Math.Max(0m, verifiedPayments - completedRefunds);

            foreach (var payment in _db.Payments.Where(item =>
                item.BookingId == id && item.PaymentStatus == "pending"))
            {
                payment.PaymentStatus = "rejected";
                payment.ReviewedBy = User.Identity?.Name;
                payment.ReviewedAt = DateTime.Now;
                payment.Note = "รายการจองถูกยกเลิกโดยผู้ดูแลระบบ";
            }

            booking.BookingStatus = "cancelled";
            if (refundDue > 0m)
            {
                _db.Payments.Add(new PaymentModel
                {
                    BookingId = id,
                    PaymentType = "refund",
                    Amount = refundDue,
                    PaymentStatus = "pending",
                    Note = "คืนเงินเต็มจำนวนจากการยกเลิกการจองโดยผู้ดูแลระบบ",
                    CreatedAt = DateTime.Now
                });
            }

            _db.SaveChanges();
            transaction.Commit();
            TempData["AdminBookingMessage"] = $"ยกเลิกรายการจอง #{booking.BookingId} แล้ว";
            if (refundDue > 0m)
            {
                TempData["AdminBookingMessage"] += $" ส่งคำขอคืนเงิน ฿{refundDue:N2} ให้ฝ่ายการเงินแล้ว";
            }
            return RedirectToAction(nameof(Bookings), new { userId = booking.UserId });
        }

        // ====================================================
        // 3. เพิ่มรถยนต์ใหม่ (POST)
        // ====================================================
        [HttpPost]
        public IActionResult AddCar(CarModel car)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(car.CarName) ||
                car.CarName.Length > 150 ||
                string.IsNullOrWhiteSpace(car.ImageUrl) ||
                car.ImageUrl.Length > 255 ||
                string.IsNullOrWhiteSpace(car.Details) ||
                car.Details.Length > 255 ||
                car.PricePerDay <= 0 ||
                car.Seats is < 2 or > 12 ||
                !new[] { "automatic", "manual" }.Contains(car.TransmissionType) ||
                !new[] { "ว่าง", "ไม่ว่าง", "อยู่ระหว่างซ่อม" }.Contains(car.Status))
            {
                TempData["CarError"] = "ข้อมูลรถไม่ถูกต้อง กรุณาตรวจสอบข้อมูลและลองอีกครั้ง";
                return RedirectToAction("CarManagement");
            }

            car.CarName = car.CarName.Trim();
            car.ImageUrl = car.ImageUrl.Trim();
            car.Details = car.Details.Trim();
            _db.Set<CarModel>().Add(car);
            _db.SaveChanges();
            return RedirectToAction("CarManagement");
        }

        // ====================================================
        // 4. แก้ไขข้อมูลรถยนต์ (POST)
        // ====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCar(CarModel car)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (car.CarId <= 0 ||
                string.IsNullOrWhiteSpace(car.CarName) || car.CarName.Length > 150 ||
                string.IsNullOrWhiteSpace(car.ImageUrl) || car.ImageUrl.Length > 255 ||
                string.IsNullOrWhiteSpace(car.Details) || car.Details.Length > 255 ||
                car.PricePerDay <= 0 ||
                car.Seats is < 2 or > 12 ||
                !new[] { "automatic", "manual" }.Contains(car.TransmissionType) ||
                !new[] { "ว่าง", "ไม่ว่าง", "อยู่ระหว่างซ่อม" }.Contains(car.Status))
            {
                TempData["CarError"] = "ข้อมูลรถไม่ถูกต้อง กรุณาตรวจสอบข้อมูลและลองอีกครั้ง";
                return RedirectToAction("CarManagement");
            }

            var existingCar = _db.Set<CarModel>().Find(car.CarId);
            if (existingCar == null)
            {
                TempData["CarError"] = "ไม่พบรถยนต์ที่ต้องการแก้ไข";
                return RedirectToAction("CarManagement");
            }

            existingCar.CarName = car.CarName.Trim();
            existingCar.PricePerDay = car.PricePerDay;
            existingCar.ImageUrl = car.ImageUrl.Trim();
            existingCar.Status = car.Status;
            existingCar.Details = car.Details.Trim();
            existingCar.TransmissionType = car.TransmissionType;
            existingCar.Seats = car.Seats;
            _db.SaveChanges();

            TempData["CarSuccess"] = "บันทึกการแก้ไขข้อมูลรถเรียบร้อยแล้ว";
            return RedirectToAction("CarManagement");
        }

        // ====================================================
        // 5. ลบรถยนต์ (GET/POST)
        // ====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCar(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var car = _db.Set<CarModel>().Find(id);
            if (car != null)
            {
                if (_db.Bookings.Any(booking => booking.CarId == id))
                {
                    TempData["CarError"] = "รถที่มีประวัติการจองไม่สามารถลบได้";
                    return RedirectToAction("CarManagement");
                }
                _db.Set<CarModel>().Remove(car);
                _db.SaveChanges();
            }
            return RedirectToAction("CarManagement");
        }

        private bool CanEditBooking(BookingModel booking)
        {
            return (booking.BookingStatus is "pending_payment" or "payment_rejected") &&
                !_db.Payments.Any(payment => payment.BookingId == booking.BookingId &&
                    (payment.PaymentStatus == "pending" ||
                     payment.PaymentStatus == "verified" ||
                     payment.PaymentStatus == "completed"));
        }

        private bool CanCancelBooking(BookingModel booking)
        {
            return booking.BookingStatus is "pending" or "pending_payment" or "payment_rejected" or "payment_review" or "confirmed";
        }

        private bool TryCalculateUpdatedPrice(
            BookingModel booking,
            CarModel car,
            DateTime pickupDate,
            int rentalDays,
            out decimal discountAmount,
            out decimal addonTotal,
            out decimal totalPrice)
        {
            discountAmount = 0m;
            addonTotal = 0m;
            totalPrice = 0m;

            if (!string.IsNullOrWhiteSpace(booking.PromotionCode))
            {
                var promotion = _db.Promotions
                    .Where(item => item.IsActive)
                    .ToList()
                    .FirstOrDefault(item => string.Equals(
                        item.PromotionCode,
                        booking.PromotionCode,
                        StringComparison.OrdinalIgnoreCase));
                if (promotion is null ||
                    !IsPromotionEligibleForUpdatedBooking(promotion, booking, pickupDate, rentalDays))
                {
                    return false;
                }

                discountAmount = Math.Round(
                    rentalDays * car.PricePerDay * promotion.DiscountPercent / 100m,
                    2,
                    MidpointRounding.AwayFromZero);
            }

            decimal addonPerDay = booking.RentalDays > 0
                ? booking.AddonTotal / booking.RentalDays
                : 0m;
            addonTotal = Math.Round(addonPerDay * rentalDays, 2, MidpointRounding.AwayFromZero);
            totalPrice = rentalDays * car.PricePerDay - discountAmount + addonTotal;
            return totalPrice > 0m && totalPrice <= 99999999.99m;
        }

        private bool IsPromotionEligibleForUpdatedBooking(
            PromotionModel promotion,
            BookingModel booking,
            DateTime pickupDate,
            int rentalDays)
        {
            DateTime lastRentalDate = pickupDate.AddDays(rentalDays - 1);
            if ((promotion.StartDate.HasValue && promotion.StartDate.Value.Date > pickupDate) ||
                (promotion.EndDate.HasValue && promotion.EndDate.Value.Date < lastRentalDate))
            {
                return false;
            }

            bool isNewCustomer = !_db.Bookings.Any(item =>
                item.BookingId != booking.BookingId &&
                item.UserId == booking.UserId &&
                item.BookingStatus != "cancelled" &&
                item.BookingStatus != "payment_rejected");
            return promotion.ConditionType switch
            {
                "new_user" => isNewCustomer,
                "3_days" => rentalDays >= 3,
                "7_days" => rentalDays >= 7,
                "weekday" => Enumerable.Range(0, rentalDays)
                    .All(offset => pickupDate.AddDays(offset).DayOfWeek is >= DayOfWeek.Monday and <= DayOfWeek.Thursday),
                "festival" => promotion.StartDate.HasValue &&
                    promotion.EndDate.HasValue &&
                    pickupDate >= promotion.StartDate.Value.Date &&
                    lastRentalDate <= promotion.EndDate.Value.Date,
                "custom" => true,
                _ => false
            };
        }
    }
}