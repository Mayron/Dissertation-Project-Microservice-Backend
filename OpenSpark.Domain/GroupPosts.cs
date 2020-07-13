using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class GroupPosts
    {
        public string GroupId { get; set; }
        public bool IsPublic { get; set; }
        public List<Post> Posts { get; set; }
    }
}
