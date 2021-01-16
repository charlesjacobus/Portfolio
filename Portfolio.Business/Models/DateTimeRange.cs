using System;

namespace Portfolio.Business.Models
{
    public class DateTimeRange
    {
        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public static DateTimeRange Create(DateTime startDateTime, DateTime endDateTime, bool forceChronological = false)
        {
            var dateTimeRange = new DateTimeRange
            {
                StartDateTime = startDateTime,
                EndDateTime = endDateTime
            };

            if (forceChronological && dateTimeRange.StartDateTime > dateTimeRange.EndDateTime)
            {
                dateTimeRange = Reverse(dateTimeRange);
            }

            return dateTimeRange;
        }

        public static DateTimeRange Reverse(DateTimeRange dateTimeRange)
        {
            if (dateTimeRange == null)
            {
                return null;
            }

            var d = dateTimeRange.StartDateTime;
            dateTimeRange.StartDateTime = dateTimeRange.EndDateTime;
            dateTimeRange.EndDateTime = d;

            return dateTimeRange;
        }
    }
}
