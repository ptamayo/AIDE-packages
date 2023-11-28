using System;

namespace Aide.Core.Domain
{
    public class LocalDateTimeResult
    {
        public bool IsOperationSuccessful { get; set; }
        public string Message { get; set; }
        public DateTime LocalDatetime { get; set; }

        public LocalDateTimeResult()
        {
            IsOperationSuccessful = false;
            LocalDatetime = default;
            Message = null;
        }
    }
}
