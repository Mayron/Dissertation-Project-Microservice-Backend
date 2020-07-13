using System.Collections.Generic;
using OpenSpark.Shared.ViewModels;

namespace OpenSpark.Shared.Payloads
{
    public class NewsFeedPostsPayload
    {
        public string ConnectionId { get; set; }

        public IList<PostViewModel> Posts { get; set; }
    }
}