using System;
using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class Member
    {
        public string Id { get; set; }
        public string AuthUserId { get; set; }
        // Should not contain implicit roles!
        public List<Guid> RoleIds { get; set; }
        public string GroupId { get; set; }
        public int Contribution { get; set; }
        public DateTime Joined { get; set; }
        public DateTime LastContribution { get; set; }
    }
}