using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Events.Sagas.CreateGroup
{
    public class GroupCreatedEvent : ISagaEvent
    {
        public Guid TransactionId { get; set; }
        public Group Group { get; set; }
    }
}