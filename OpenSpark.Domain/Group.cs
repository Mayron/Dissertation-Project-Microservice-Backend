using System;
using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class Group : INamedEntity
    {
        public string Visibility { get; set; }
        public List<string> Members { get; set; }
        public List<string> BannedUsers { get; set; }
        public List<Role> Roles { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public string CategoryId { get; set; }
        public List<string> Tags { get; set; }
        public string OwnerUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}