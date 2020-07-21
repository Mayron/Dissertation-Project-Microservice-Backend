using System;
using OpenSpark.Shared.Queries;

namespace OpenSpark.Shared.Events.Payloads
{
    public class PayloadEvent : IPayloadEvent
    {
        public object Payload { get; set; }
        public string Callback { get; set; }
        public string ConnectionId { get; set; }
        public string[] Errors { get; set; }
        public Guid MultiQueryId { get; set; }
        public Guid QueryId { get; set; }

        public void Deconstruct(out string connectionId, out string callback, out object eventData)
        {
            connectionId = ConnectionId;
            callback = Callback;
            eventData = new { errors = Errors, payload = Payload };
        }

        public PayloadEvent() {}

        public PayloadEvent(IQuery query)
        {
            Callback     = query.Callback;
            ConnectionId = query.ConnectionId;
            MultiQueryId = query.MultiQueryId;
            QueryId      = query.Id;
        }
    }
}