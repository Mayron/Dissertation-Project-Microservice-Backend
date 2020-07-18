using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Events.Sagas.CreateProject
{
    public class ProjectCreatedEvent : ISagaEvent
    {
        public Guid TransactionId { get; set; }
        public Project Project { get; set; }
    }
}