using System;

namespace OpenSpark.Shared.Events.Sagas
{
    public class UserVerifiedEvent : ISagaEvent
    {
        public Guid TransactionId { get; set; }
    }
}
