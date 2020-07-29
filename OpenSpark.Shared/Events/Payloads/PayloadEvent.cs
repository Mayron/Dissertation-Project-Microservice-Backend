namespace OpenSpark.Shared.Events.Payloads
{
    public class PayloadEvent : IPayloadEvent
    {
        public MetaData MetaData { get; set; }
        public object Payload { get; set; }
        public string[] Errors { get; set; }

        public void Deconstruct(out string connectionId, out string clientSideMethod, out object eventData)
        {
            connectionId = MetaData.ConnectionId;
            clientSideMethod = MetaData.Callback;
            eventData = new { errors = Errors, payload = Payload };
        }

        public PayloadEvent()
        {
        }

        public PayloadEvent(IMessage message) => MetaData = message.MetaData;
    }
}