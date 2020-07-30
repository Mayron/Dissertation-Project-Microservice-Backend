using System;

namespace OpenSpark.Shared.Events.CreateGroup
{
    public class GroupCreatedEvent
    {
        public string GroupId { get; set; }
        public string GroupVisibility { get; set; }
    }
}