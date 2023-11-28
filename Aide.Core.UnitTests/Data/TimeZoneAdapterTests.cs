using Aide.Core.Adapters;
using Aide.Core.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aide.Core.UnitTests.Data
{
    [TestFixture]
    public class TimeZoneAdapterTests
    {
        private ITimeZoneAdapter _adapter;

        [SetUp]
        public void Setup()
        {
            _adapter = new TimeZoneAdapter();
        }

        #region GetLocalDateTime

        [Test]
        public void GetLocalDateTime_WhenValidIanaTimeZoneProvided_ThenReturnTheCorrespondingDateTime()
        {
            #region Arrange

            // Dev notes: Hawaii it's been selected for this test because this time zone does not observe DST (daylight saving time),
            // in other words, it doesn't change like the Pacific Time does.
            string localIanaTimezone = "Pacific/Honolulu";

            #endregion

            #region Act

            var result = _adapter.GetLocalDateTime(localIanaTimezone);

            #endregion

            #region Assert

            // Verify the result contains a datetime which corresponds to the conversion from UTC to PST.
            Assert.AreEqual(DateTime.UtcNow.AddHours(-10).ToString("yyyy-MM-dd HH"), result.LocalDatetime.ToString("yyyy-MM-dd HH"));

            #endregion
        }

        #endregion
    }
}
