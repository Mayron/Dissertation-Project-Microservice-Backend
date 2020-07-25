using System;

namespace OpenSpark.Shared.ViewModels
{
    public class ProjectDetailsViewModel
    {
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public int TotalSubscribers { get; set; }
        public string Visibility { get; set; }
        public string ConnectedGroupId { get; set; }
        public bool Subscribed { get; set; }
        public int TotalDownloads { get; set; }
        public string LastUpdated { get; set; }
        public bool IsOwner { get; set; }
    }
}