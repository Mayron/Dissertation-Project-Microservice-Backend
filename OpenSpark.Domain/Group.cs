using System;
using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class Group
    {
        public string Visibility { get; set; }
        public List<Member> Members { get; set; }
        public List<Member> BannedUsers { get; set; }
        public List<Role> Roles { get; set; }
        public string GroupId { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public string CategoryId { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Connected { get; set; }
        public string OwnerUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}