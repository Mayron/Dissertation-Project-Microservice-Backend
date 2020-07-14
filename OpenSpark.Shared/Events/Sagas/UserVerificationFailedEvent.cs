using System;

namespace OpenSpark.Shared.Events.Sagas
{
    public class UserVerificationFailedEvent : ISagaEvent
    {
        public Guid TransactionId { get; set; }
    }
}
