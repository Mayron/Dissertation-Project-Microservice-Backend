namespace OpenSpark.Shared.Events.ConnectProject
{
    public class ProjectFailedToConnectEvent
    {
        public string ProjectId { get; set; }
        public string Message { get; set; }
    }
}