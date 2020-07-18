using System;

namespace OpenSpark.Shared.Events.Sagas.CreatePost
{
    public class ProjectDeletedEvent : ISagaEvent
    {
        public Guid TransactionId { get; set; }
        public string ProjectId { get; set; }
    }
}