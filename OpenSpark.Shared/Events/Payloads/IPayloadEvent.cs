namespace OpenSpark.Shared.Events.Payloads
{
    public interface IPayloadEvent
    {
        MetaData MetaData { get; set; }
        object Payload { get; set; }
        string[] Errors { get; set; }
    }
}