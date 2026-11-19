namespace warat_0583_l005.Services;

public static class CarLocationOptions
{
    public static readonly string[] All =
    [
        "สนามบินดอนเมือง",
        "สนามบินสุวรรณภูมิ",
        "เชียงใหม่",
        "ภูเก็ต"
    ];

    public static bool IsValid(string? location)
    {
        return location is not null && Array.IndexOf(All, location) >= 0;
    }
}
