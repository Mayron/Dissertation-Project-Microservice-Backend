using OpenSpark.Domain;

namespace OpenSpark.Shared.Events.CreateProject
{
    public class ProjectCreatedEvent
    {
        public Project Project { get; set; }
    }
}