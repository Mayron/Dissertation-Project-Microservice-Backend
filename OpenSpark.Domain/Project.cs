using System;
using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class Project : INamedEntity
    {
        public string Id { get; set; }
        public string About { get; set; }
        public string ConnectedGroupId { get; set; }
        public string Visibility { get; set; }
        public string Name { get; set; }
        public string OwnerUserId { get; set; }
        public List<string> Subscribers { get; set; }
        public List<string> Tags { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}