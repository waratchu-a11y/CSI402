using System.Globalization;
using warat_0583_l005.Models;

namespace warat_0583_l005.Services;

public static class CarAvailability
{
    private static readonly string[] DateFormats =
    [
        "yyyy-MM-dd",
        "M/d/yyyy",
        "MM/dd/yyyy",
        "d/M/yyyy",
        "dd/MM/yyyy"
    ];

    public static bool IsAvailable(
        CarModel car,
        IEnumerable<BookingModel> activeBookings,
        DateTime pickupDate,
        DateTime returnDate)
    {
        if (car.Status != "ว่าง" || returnDate.Date <= pickupDate.Date)
        {
            return false;
        }

        foreach (var booking in activeBookings)
        {
            bool belongsToCar = booking.CarId == car.CarId ||
                (!booking.CarId.HasValue && booking.CarModel == car.CarName);
            if (!belongsToCar)
            {
                continue;
            }

            if (!TryParseDate(booking.PickupDate, out DateTime existingPickup) ||
                booking.RentalDays < 1)
            {
                return false;
            }

            DateTime existingReturn = booking.ReturnDate.HasValue
                ? NormalizeYear(booking.ReturnDate.Value).Date
                : existingPickup.Date.AddDays(booking.RentalDays);

            if (existingReturn <= existingPickup.Date)
            {
                return false;
            }

            if (pickupDate.Date < existingReturn && existingPickup.Date < returnDate.Date)
            {
                return false;
            }
        }

        return true;
    }

    public static bool TryParseDate(string? value, out DateTime date)
    {
        if (DateTime.TryParseExact(
                value,
                DateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date))
        {
            date = NormalizeYear(date).Date;
            return true;
        }

        date = default;
        return false;
    }

    private static DateTime NormalizeYear(DateTime value)
    {
        while (value.Year > 2400 && value.Year <= 5000)
        {
            value = value.AddYears(-543);
        }

        return value;
    }
}
