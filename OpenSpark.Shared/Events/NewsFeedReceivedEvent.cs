using System.Collections.Generic;
using OpenSpark.Shared.ViewModels;

namespace OpenSpark.Shared.Events
{
    public class NewsFeedReceivedEvent
    {
        public string ConnectionId { get; set; }

        public IList<PostViewModel> Posts { get; set; }
    }
}