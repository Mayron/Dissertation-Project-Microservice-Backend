using System;

namespace OpenSpark.Shared.Events.Sagas
{
    public class ProjectFailedToConnectEvent
    {
        public Guid TransactionId { get; set; }
        public string ProjectId { get; set; }
        public string Message { get; set; }
    }
}