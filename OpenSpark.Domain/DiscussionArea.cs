using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class DiscussionArea
    {
        // Can be the projectId or groupId
        public string AreaId { get; set; }
        public List<Post> Posts { get; set; }
        public bool IsPublic { get; set; }
    }
}
