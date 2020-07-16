using System;

namespace OpenSpark.Shared.Events.Sagas
{
    public class SagaErrorEvent : ISagaEvent
    {
        public string Message { get; set; }
        public Guid TransactionId { get; set; }
    }
}