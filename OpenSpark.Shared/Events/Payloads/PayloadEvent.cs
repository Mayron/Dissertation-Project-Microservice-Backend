using System;

namespace OpenSpark.Shared.Events.Payloads
{
    public class PayloadEvent : IPayloadEvent
    {
        public object Payload { get; set; }
        public string Callback { get; set; }
        public string ConnectionId { get; set; }
        public string[] Errors { get; set; }
        public Guid QueryId { get; set; }

        public void Deconstruct(out string connectionId, out string callback, out object eventData)
        {
            connectionId = ConnectionId;
            callback = Callback;
            eventData = new { errors = Errors, payload = Payload };
        }
    }
}