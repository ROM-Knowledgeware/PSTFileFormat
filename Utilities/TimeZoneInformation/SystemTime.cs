using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    public class SystemTime // SYSTEMTIME
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;

        public SystemTime()
        { 
        }

        public SystemTime(byte[] buffer, int offset)
        {
            wYear = LittleEndianConverter.ToUInt16(buffer, offset + 0);
            wMonth = LittleEndianConverter.ToUInt16(buffer, offset + 2);
            wDayOfWeek = LittleEndianConverter.ToUInt16(buffer, offset + 4);
            wDay = LittleEndianConverter.ToUInt16(buffer, offset + 6);
            wHour = LittleEndianConverter.ToUInt16(buffer, offset + 8);
            wMinute = LittleEndianConverter.ToUInt16(buffer, offset + 10);
            wSecond = LittleEndianConverter.ToUInt16(buffer, offset + 12);
            wMilliseconds = LittleEndianConverter.ToUInt16(buffer, offset + 14);
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            LittleEndianWriter.WriteUInt16(buffer, offset + 0, wYear);
            LittleEndianWriter.WriteUInt16(buffer, offset + 2, wMonth);
            LittleEndianWriter.WriteUInt16(buffer, offset + 4, wDayOfWeek);
            LittleEndianWriter.WriteUInt16(buffer, offset + 6, wDay);
            LittleEndianWriter.WriteUInt16(buffer, offset + 8, wHour);
            LittleEndianWriter.WriteUInt16(buffer, offset + 10, wMinute);
            LittleEndianWriter.WriteUInt16(buffer, offset + 12, wSecond);
            LittleEndianWriter.WriteUInt16(buffer, offset + 14, wMilliseconds);
        }

        public TimeSpan TimeOfDay
        {
            get
            {
                return new TimeSpan(0, wHour, wMinute, wSecond, wMilliseconds);
            }
            set
            {
                wHour = (ushort)value.Hours;
                wMinute = (ushort)value.Minutes;
                wSecond = (ushort)value.Seconds;
                wMilliseconds = (ushort)value.Milliseconds;
            }
        }

        public DayOfWeek DayOfWeek
        {
            get
            { 
                return (DayOfWeek)wDayOfWeek;
            }
        }

        public DateTime DateTime
        {
            get
            {
                return new DateTime(wYear, wMonth, wDay, wHour, wMinute, wSecond, wMilliseconds);
            }
            set
            {
                wYear = (ushort)value.Year;
                wMonth = (ushort)value.Month;
                wDayOfWeek = (ushort)value.DayOfWeek;
                wDay = (ushort)value.Day;
                this.TimeOfDay = value.TimeOfDay;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is SystemTime)
            {
                SystemTime a = this;
                SystemTime b = (SystemTime)obj;
                return (a.wYear == b.wYear &&
                        a.wMonth == b.wMonth &&
                        a.wDayOfWeek == b.wDayOfWeek &&
                        a.wDay == b.wDay &&
                        a.wHour == b.wHour &&
                        a.wMinute == b.wMinute &&
                        a.wSecond == b.wSecond &&
                        a.wMilliseconds == b.wMilliseconds);
            }
            return false;
        }

        public static bool operator ==(SystemTime a, SystemTime b)
        {
            return (a.Equals(b));
        }

        public static bool operator !=(SystemTime a, SystemTime b)
        {
            return (!a.Equals(b));
        }

        public override int GetHashCode()
        {
            return wYear.GetHashCode();
        }
    }
}
