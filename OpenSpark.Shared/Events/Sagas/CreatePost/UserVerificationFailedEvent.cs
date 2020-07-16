using System;

namespace OpenSpark.Shared.Events.Sagas.CreatePost
{
    public class UserVerificationFailedEvent : ISagaEvent
    {
        public Guid TransactionId { get; set; }
    }
}
