using System;

namespace OpenSpark.Shared.Events.Sagas.CreateGroup
{
    public class GroupDeletedEvent : ISagaEvent
    {
        public Guid TransactionId { get; set; }
        public string GroupId { get; set; }
    }
}