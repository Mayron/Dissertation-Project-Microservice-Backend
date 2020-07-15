using System;

namespace OpenSpark.Shared.Events.Sagas
{
    public class SagaFinishedEvent : ISagaEvent
    {
        public Guid TransactionId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}