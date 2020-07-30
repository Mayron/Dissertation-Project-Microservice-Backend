using System;
using System.Collections.Generic;
using OpenSpark.Shared.Domain;

namespace OpenSpark.Projects.Domain
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
        public List<string> TeamMembers { get; set; }
        public DateTime LastUpdated { get; set; }
        public int TotalDownloads { get; set; }
        public List<string> LinkedGroups { get; set; }
    }
}