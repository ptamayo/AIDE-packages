using Aide.Core.Domain;
using Aide.Core.Interfaces;
using System;
using TimeZoneConverter;

namespace Aide.Core.Adapters
{
    /// <summary>
    /// Adapter for converting IANA timezone names to Windows.
    /// For further details see:
    /// https://github.com/mattjohnsonpint/TimeZoneConverter
    /// </summary>
    public class TimeZoneAdapter : ITimeZoneAdapter
    {
        public TimeZoneInfo GetTimeZoneInfo(string windowsOrIanaTimezoneId)
        {
            return TZConvert.GetTimeZoneInfo(windowsOrIanaTimezoneId);
        }

        /// <summary>
        /// Returns the result of converting the current UTC Date/Time to the IANA Time Zone provided.
        /// </summary>
        /// <param name="localIanaTimezone">string</param>
        /// <returns>LocalDateTimeResult</returns>
        public LocalDateTimeResult GetLocalDateTime(string localIanaTimezone)
        {
            TimeZoneInfo tzInfo;
            try
            {
                tzInfo = GetTimeZoneInfo(localIanaTimezone);
            }
            catch
            {
                var errorMessage = $"Failed to get the local datetime for the given Iana TimeZone of {localIanaTimezone}";
                return new LocalDateTimeResult
                {
                    IsOperationSuccessful = false,
                    Message = errorMessage
                };
            }

            return new LocalDateTimeResult
            {
                IsOperationSuccessful = true,
                LocalDatetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzInfo)
            };
        }
    }
}
