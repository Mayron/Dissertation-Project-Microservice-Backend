using OpenSpark.Shared.Events.Payloads;
using System;
using System.Collections.Immutable;

namespace OpenSpark.Shared.Events
{
    public class MultiPayloadEvent
    {
        public Guid MultiQueryId { get; set; }
        public IImmutableDictionary<Guid, IPayloadEvent> Payloads { get; set;  }
    }
}