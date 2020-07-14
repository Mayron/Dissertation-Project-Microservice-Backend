using OpenSpark.Shared.ViewModels;

namespace OpenSpark.Shared.Events
{
    public class PostAddedEvent
    {
        public string GroupId { get; set; }
        public PostViewModel Post { get; set; }
    }
}
