using System.Collections.Generic;
using OpenSpark.Domain;

namespace OpenSpark.Discussions.Payloads
{
    public class NewsFeedPostsPayload
    {
        public string ConnectionId { get; set; }

        public List<Post> Posts { get; set; }
    }
}