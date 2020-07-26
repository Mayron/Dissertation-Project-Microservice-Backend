using System;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;

namespace OpenSpark.ApiGateway.Builders
{
    public class QueryContext
    {
        public int RemoteSystemId { get; set; }
        public IQuery Query { get; set; }
        public Action<PayloadEvent> OnPayloadReceived { get; set; }
    }
}