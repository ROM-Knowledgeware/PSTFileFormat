using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    public class DateTimeUtils
    {
        public static bool IsInDateRange(DateTime from, DateTime to, DateTime dt)
        {
            return (dt <= to && dt >= from);
        }

        public static bool IsInDay(DateTime day, DateTime dt)
        {
            return (day.Year == dt.Year && day.Month == dt.Month && day.Day == dt.Day);
        }

        public static DateTime GetDayStart(DateTime day)
        {
            DateTime result = new DateTime(day.Year, day.Month, day.Day);
            result = DateTime.SpecifyKind(result, day.Kind);
            return result;
        }

        public static DateTime GetDayEnd(DateTime day)
        {
            DateTime result = new DateTime(day.Year, day.Month, day.Day);
            // We can't use result.AddDays(1).AddTicks(-1) on DateTime.MaxValue.Date
            result = result.AddTicks(new TimeSpan(24, 0, 0).Ticks - 1);
            result = DateTime.SpecifyKind(result, day.Kind);
            return result;
        }

        public static DateTime GetWeekStart(DateTime dt)
        {
            dt = dt.AddDays(-(int)dt.DayOfWeek);
            return GetDayStart(dt);
        }

        public static DateTime GetWeekEnd(DateTime dt)
        {
            dt = dt.AddDays(6 - (int)dt.DayOfWeek);
            return GetDayEnd(dt);
        }

        public static DateTime GetMonthStart(DateTime dt)
        {
            DateTime result = new DateTime(dt.Year, dt.Month, 1);
            result = DateTime.SpecifyKind(result, dt.Kind);
            return result;
        }

        public static DateTime GetMonthEnd(DateTime dt)
        {
            DateTime result = GetMonthStart(dt);
            result = result.AddMonths(1);
            result = result.AddTicks(-1);
            result = DateTime.SpecifyKind(result, dt.Kind);
            return result;
        }

        public static DateTime GetMonthStart(int year, int month)
        {
            return new DateTime(year, month, 1);
        }

        public static DateTime GetMonthEnd(int year, int month)
        {
            DateTime result = GetMonthStart(year, month);
            result = result.AddMonths(1);
            result = result.AddTicks(-1);
            return result;
        }

        public static DateTime GetYearStart(int year)
        {
            return GetMonthStart(year, 1);
        }

        public static DateTime GetYearEnd(int year)
        {
            return GetMonthEnd(year, 12);
        }

        public static List<DateTime> GetDaysInRange(DateTime from, DateTime to)
        {
            List<DateTime> result = new List<DateTime>();
            from = GetDayStart(from);
            to = GetDayStart(to);
            if (from <= to)
            {
                DateTime current = GetDayStart(from);
                while (current <= to)
                {
                    result.Add(current);
                    current = current.AddDays(1);
                }
            }
            return result;
        }

        public static List<DateTime> GetWeeksInRange(DateTime from, DateTime to)
        {
            List<DateTime> result = new List<DateTime>();
            from = GetWeekStart(from);
            to = GetWeekStart(to);
            if (from <= to)
            {
                DateTime current = GetWeekStart(from);
                while (current <= to)
                {
                    result.Add(current);
                    current = current.AddDays(7);
                }
            }
            return result;
        }

        public static List<DateTime> GetMonthsInRange(DateTime from, DateTime to)
        {
            List<DateTime> result = new List<DateTime>();
            from = GetMonthStart(from);
            to = GetMonthStart(to);
            if (from <= to)
            {
                DateTime current = GetMonthStart(from);
                while (current <= to)
                {
                    result.Add(current);
                    current = current.AddMonths(1);
                }
            }
            return result;
        }

        public static int GetNumberOfMonthsInRange(DateTime from, DateTime to)
        {
            return 12 * (to.Year - from.Year) + to.Month - from.Month + 1;
        }

        public static DateTime SetTimeOfDay(DateTime dt, TimeSpan timeOfDay)
        {
            dt = DateTimeUtils.GetDayStart(dt);
            dt = dt.Add(timeOfDay);
            return dt;
        }
    }
}
