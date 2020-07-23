using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Events.CreateGroup
{
    public class GroupCreatedEvent
    {
        public Group Group { get; set; }
    }
}