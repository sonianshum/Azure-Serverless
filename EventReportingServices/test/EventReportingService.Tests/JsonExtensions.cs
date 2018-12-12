namespace EventReportingService.Tests
{
    using System;

    using EventReportingService.Utilities;

    public static class JsonExtensions
    {
        public static string ToJson<T>(this T gridEvent)
        {
            return Json.SerializeObject(gridEvent);
        }

        public static DateTime Normalize(this DateTime dateTime)
        {
            return Json.DeserializeObject<DateTime>(Json.SerializeObject(dateTime));
        }
    }
}
