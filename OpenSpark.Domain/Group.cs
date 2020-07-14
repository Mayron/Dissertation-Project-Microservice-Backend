using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class Group
    {
        public string VisibilityStatus { get; set; }
        public List<Member> Members { get; set; }
        public List<Member> BannedUsers { get; set; }
        public List<Role> Roles { get; set; }
        public string GroupId { get; set; }
    }
}