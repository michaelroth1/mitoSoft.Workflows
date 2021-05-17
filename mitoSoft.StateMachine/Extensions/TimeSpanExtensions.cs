using System;

namespace mitoSoft.Workflows.Extensions
{
    internal static class TimeSpanExtensions
    {
        public static DateTime GetTime(this TimeSpan timeout)
        {
            if (timeout > TimeSpan.Zero)
            {
                return DateTime.UtcNow.Add(timeout);
            }
            else
            {
                return new DateTime();
            }
        }
    }
}