using Aide.Core.Domain;
using System;

namespace Aide.Core.Interfaces
{
    public interface ITimeZoneAdapter
    {
        TimeZoneInfo GetTimeZoneInfo(string windowsOrIanaTimezoneId);
        LocalDateTimeResult GetLocalDateTime(string localIanaTimezone);
    }
}
