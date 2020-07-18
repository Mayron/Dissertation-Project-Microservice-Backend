using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class Project
    {
        public string Id { get; set; }
        public string ConnectedGroupId { get; set; }
        public string VisibilityStatus { get; set; }
        public string Name { get; set; }
        public string OwnerUserId { get; set; }
        public List<string> Subscribers { get; set; }
    }
}