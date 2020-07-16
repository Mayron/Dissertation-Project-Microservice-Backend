using System;

namespace OpenSpark.Shared.Events.Sagas.CreatePost
{
    public class UserVerifiedEvent : ISagaEvent
    {
        public Guid TransactionId { get; set; }
    }
}
