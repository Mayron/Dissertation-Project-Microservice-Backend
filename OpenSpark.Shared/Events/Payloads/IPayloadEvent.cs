using OpenSpark.Shared.Queries;

namespace OpenSpark.Shared.Events.Payloads
{
    public interface IPayloadEvent
    {
        QueryMetaData MetaData { get; set; }
        object Payload { get; set; }
        string[] Errors { get; set; }
    }
}
