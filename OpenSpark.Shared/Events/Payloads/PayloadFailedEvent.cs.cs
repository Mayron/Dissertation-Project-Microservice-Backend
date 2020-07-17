namespace OpenSpark.Shared.Events.Payloads
{
    public class PayloadEvent
    {
        public object Payload { get; set; }
        public string Callback { get; set; }
        public string ConnectionId { get; set; }
        public string Error { get; set; }

        public void Deconstruct(out string connectionId, out string callback, out object eventData)
        {
            connectionId = ConnectionId;
            callback = Callback;
            eventData = new { error = Error, payload = Payload };
        }
    }
}