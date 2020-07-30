using System;
using System.Collections.Generic;
using OpenSpark.Shared.Domain;

namespace OpenSpark.Groups.Domain
{
    public class Member : IEntity
    {
        public string Id { get; set; }
        public string AuthUserId { get; set; }
        // Should not contain implicit roles!
        public List<string> RoleIds { get; set; }
        public string GroupId { get; set; }
        public int Contribution { get; set; }
        public DateTime Joined { get; set; }
        public DateTime LastContribution { get; set; }
    }
}