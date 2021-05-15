using System;

namespace mitoSoft.Workflows.Extensions
{
    internal static class DateTimeExtensions
    {
        public static void ThrowIfTimeExceeded(this DateTime value)
        {
            if (new DateTime() < value && value < DateTime.UtcNow)
            {
                throw new TimeoutException("Timeout");
            }
        }
    }
}