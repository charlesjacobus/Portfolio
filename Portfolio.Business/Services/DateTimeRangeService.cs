using System;
using System.Text.RegularExpressions;

using Portfolio.Business.Models;
using static Portfolio.Business.Services.DateTimeRangeService;

namespace Portfolio.Business.Services
{
    public interface IDateTimeRangeService
    {
        DateTimeRange GetDateTimeRange(DateTime startDate, DateTime endDate, TimeHandlingOption timeHandling = TimeHandlingOption.LastSecondOfDay);

        DateTimeRange GetDateTimeRange(string startDate = null, string endDate = null, TimeHandlingOption timeHandling = TimeHandlingOption.LastSecondOfDay);

        DateTimeRange GetDateTimeRange(string startDate = null, string endDate = null, TimeHandlingOptions timeHandlingOptions = null);
    }

    public class DateTimeRangeService
        : IDateTimeRangeService
    {
        public enum TimeHandlingOption
        {
            StartOfDay,
            PreserveExactTime,
            LastSecondOfDay
        }

        public class TimeHandlingOptions
        {
            public TimeHandlingOptions()
            {
                StartDateOption = TimeHandlingOption.StartOfDay;
                EndDateOption = TimeHandlingOption.LastSecondOfDay;
            }

            public TimeHandlingOption StartDateOption { get; set; }

            public TimeHandlingOption EndDateOption { get; set; }

            public static TimeHandlingOptions Create(bool useExactTime = false)
            {
                var timeHandlingOptions = new TimeHandlingOptions();
                if (useExactTime)
                {
                    timeHandlingOptions.StartDateOption = TimeHandlingOption.PreserveExactTime;
                    timeHandlingOptions.EndDateOption = TimeHandlingOption.PreserveExactTime;
                }

                return timeHandlingOptions;
            }
        }

        public DateTimeRange GetDateTimeRange(DateTime startDate, DateTime endDate, TimeHandlingOption endDateTimeHandling = TimeHandlingOption.LastSecondOfDay)
        {
            var timeHandlingOptions = TimeHandlingOptions.Create();
            timeHandlingOptions.EndDateOption = endDateTimeHandling;

            return GetDateTimeRange(startDate.ToString(), endDate.ToString(), timeHandlingOptions);
        }

        public DateTimeRange GetDateTimeRange(string startDate = null, string endDate = null, TimeHandlingOption endDateTimeHandling = TimeHandlingOption.LastSecondOfDay)
        {
            var timeHandlingOptions = TimeHandlingOptions.Create();
            timeHandlingOptions.EndDateOption = endDateTimeHandling;

            return GetDateTimeRange(startDate, endDate, timeHandlingOptions);
        }

        public DateTimeRange GetDateTimeRange(string startDate = null, string endDate = null, TimeHandlingOptions timeHandlingOptions = null)
        {
            if (timeHandlingOptions == null)
            {
                timeHandlingOptions = TimeHandlingOptions.Create();
            }

            if (string.IsNullOrWhiteSpace(startDate) && string.IsNullOrWhiteSpace(endDate))
            {
                startDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (string.IsNullOrWhiteSpace(startDate) && !string.IsNullOrWhiteSpace(endDate))
            {
                startDate = endDate;
            }
            if (!string.IsNullOrWhiteSpace(startDate) && string.IsNullOrWhiteSpace(endDate))
            {
                endDate = startDate;
            }

            var startDateTime = GetDateTime(startDate, timeHandlingOptions.StartDateOption);
            var endDateTime = GetDateTime(endDate, timeHandlingOptions.EndDateOption);

            var result = DateTimeRange.Create(startDateTime, endDateTime);

            return result;
        }

        /// <summary>
        /// Returns a <see cref="DateTimeRange"/> instance (or null) for <paramref name="partialOrCompleteIso8601DateTimeValue"/>
        /// </summary>
        /// <param name="partialOrCompleteIso8601DateTimeValue">A partial or complete ISO 8601 date-time value</param>
        /// <param name="endDateTimeHandling">A <see cref="TimeHandlingOption"/> value specifying how to handle the end date-time in the <see cref="DateTimeRange" instance</param>
        /// <returns>A <see cref="DateTimeRange"/> instance</returns>
        /// <remarks>
        /// Only ISO 8601 date-time formats are supported (e.g., 2020-07-31T14:37:17), but partial values are supported.
        /// For example, by providing a value of "2020-07", the <see cref="DateTimeRange"/> instance will have start and end <see cref="DateTime"/> values for the month of July 2020.
        /// Likewise, providing a value of just "2020" returns a <see cref="DateTimeRange"/> instance with a range spanning the year 2020.
        /// </remarks>
        public static DateTimeRange GetDateTimeRange(string partialOrCompleteIso8601DateTimeValue, TimeHandlingOption endDateTimeHandling = TimeHandlingOption.LastSecondOfDay)
        {
            var valueString = Convert.ToString(partialOrCompleteIso8601DateTimeValue);
            if (string.IsNullOrWhiteSpace(valueString))
            {
                return null;
            }

            DateTime startDate;
            DateTime endDate;
            if (Regex.Match(valueString, @"^[0-9]{4}$").Success)
            {
                startDate = new DateTime(int.Parse(valueString), 1, 1);
                endDate = new DateTime(int.Parse(valueString), 12, 31);
            }
            else if (Regex.Match(valueString, @"^[0-9]{4}-[0-9]{2}$").Success)
            {
                startDate = DateTime.Parse($"{valueString}-01");
                endDate = new DateTime(startDate.Year, startDate.Month, DateTime.DaysInMonth(startDate.Year, startDate.Month));
            }
            else if (DateTime.TryParse(valueString, out DateTime result))
            {
                startDate = result;
                endDate = result;
            }
            else
            {
                return null;
            }

            var dateTimeRange = new DateTimeRangeService().GetDateTimeRange(
                startDate,
                endDate,
                endDateTimeHandling);

            return dateTimeRange;
        }

        internal static DateTime GetDateTime(string dateTimeValue, TimeHandlingOption timeHandling)
        {
            if (!DateTime.TryParse(dateTimeValue, out DateTime dateTimeResult))
            {
                dateTimeResult = DateTime.Now;
            }

            DateTime result;
            if (timeHandling == TimeHandlingOption.StartOfDay)
            {
                result = new DateTime(dateTimeResult.Year, dateTimeResult.Month, dateTimeResult.Day);
            }
            else if (timeHandling == TimeHandlingOption.LastSecondOfDay)
            {
                var maximumAdjustableValue = DateTime.MaxValue.AddDays(-1);
                if (dateTimeResult <= maximumAdjustableValue)
                {
                    result = new DateTime(dateTimeResult.Year, dateTimeResult.Month, dateTimeResult.Day).AddDays(1).AddSeconds(-1);
                }
                else
                {
                    result = dateTimeResult;
                }
            }
            else
            {
                result = dateTimeResult;
            }

            return result;
        }
    }
}
