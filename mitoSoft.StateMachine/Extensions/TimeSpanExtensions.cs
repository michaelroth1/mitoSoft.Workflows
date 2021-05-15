using System;

namespace mitoSoft.Workflows.Extensions
{
    internal static class TimeSpanExtensions
    {
        public static DateTime GetTime(this TimeSpan timeOut)
        {
            if (timeOut > TimeSpan.Zero)
            {
                return DateTime.UtcNow.Add(timeOut);
            }
            else
            {
                return new DateTime();
            }
        }
    }
}