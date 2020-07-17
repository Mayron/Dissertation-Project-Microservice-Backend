using System;
using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class Member
    {
        public string UserId { get; set; }
        // Should not contain implicit roles!
        public List<Guid> RoleIds { get; set; }
        public string GroupId { get; set; }
        public int Contribution { get; set; }
    }
}