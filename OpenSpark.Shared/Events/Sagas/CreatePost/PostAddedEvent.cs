using System;
using OpenSpark.Shared.ViewModels;

namespace OpenSpark.Shared.Events.Sagas.CreatePost
{
    public class PostAddedEvent : ISagaEvent
    {
        public Guid TransactionId { get; set; }
        public string GroupId { get; set; }
        public PostViewModel Post { get; set; }
    }
}
