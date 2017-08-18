using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HillsRainBotFunctions
{
    public static class DateTimeExtensions
    {
        static readonly TimeZoneInfo JapanTimeZone = System.TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

        public static DateTime JapanNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, JapanTimeZone);

        public static DateTime JapanToday => JapanNow.Date;

        public static DateTime ConvertToJapanTime(this DateTime time) => TimeZoneInfo.ConvertTime(time, JapanTimeZone);
    }
}
