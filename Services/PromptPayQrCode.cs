using System.Globalization;
using System.Text;
using QRCoder;

namespace warat_0583_l005.Services;

public static class PromptPayQrCode
{
    public static bool IsValidMobileNumber(string? mobileNumber)
    {
        return mobileNumber is { Length: 10 } &&
            mobileNumber[0] == '0' &&
            mobileNumber.All(char.IsAsciiDigit);
    }

    public static string CreatePngDataUri(string mobileNumber, decimal amount, string reference)
    {
        string payload = CreatePayload(mobileNumber, amount, reference);
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        var png = new PngByteQRCode(data).GetGraphic(10);
        return "data:image/png;base64," + Convert.ToBase64String(png);
    }

    public static string CreatePayload(string mobileNumber, decimal amount, string reference)
    {
        if (!IsValidMobileNumber(mobileNumber))
        {
            throw new ArgumentException("PromptPay ID must be a 10-digit Thai mobile number.", nameof(mobileNumber));
        }

        if (amount <= 0m || amount > 99999999.99m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be between 0.01 and 99,999,999.99.");
        }

        string normalizedMobileNumber = "0066" + mobileNumber[1..];
        string referenceLabel = new string(reference.Where(char.IsAsciiLetterOrDigit).Take(25).ToArray());
        string payload =
            Tlv("00", "01") +
            Tlv("01", "12") +
            Tlv("29", Tlv("00", "A000000677010111") + Tlv("01", normalizedMobileNumber)) +
            Tlv("52", "0000") +
            Tlv("53", "764") +
            Tlv("54", amount.ToString("0.00", CultureInfo.InvariantCulture)) +
            Tlv("58", "TH") +
            (referenceLabel.Length > 0 ? Tlv("62", Tlv("05", referenceLabel)) : string.Empty);
        payload += "6304" + Crc16(payload + "6304").ToString("X4", CultureInfo.InvariantCulture);
        return payload;
    }

    private static string Tlv(string tag, string value)
    {
        return tag + value.Length.ToString("D2", CultureInfo.InvariantCulture) + value;
    }

    private static ushort Crc16(string value)
    {
        ushort crc = 0xFFFF;
        foreach (byte valueByte in Encoding.ASCII.GetBytes(value))
        {
            crc ^= (ushort)(valueByte << 8);
            for (int bit = 0; bit < 8; bit++)
            {
                crc = (crc & 0x8000) != 0
                    ? (ushort)((crc << 1) ^ 0x1021)
                    : (ushort)(crc << 1);
            }
        }

        return crc;
    }
}
