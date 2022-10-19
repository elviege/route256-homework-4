namespace Orders.Core.Extensions;

public static class DateTimeExtensions
{
    private const string UniqueFormat = "yyyy-MM-dd HH:mm:ss.ffffff";
    public static string ToUniqueFormatString(this DateTime dt)
    {
        return dt.ToString(UniqueFormat);
    }
}