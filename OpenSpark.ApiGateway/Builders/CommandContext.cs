using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Events.Payloads;
using System;

namespace OpenSpark.ApiGateway.Builders
{
    public class CommandContext
    {
        public int RemoteSystemId { get; set; }
        public ICommand Command { get; set; }
        public Action<PayloadEvent> OnPayloadReceived { get; set; }
    }
}