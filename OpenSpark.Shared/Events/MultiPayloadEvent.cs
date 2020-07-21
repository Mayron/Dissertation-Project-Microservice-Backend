using OpenSpark.Shared.Events.Payloads;
using System;
using System.Collections.Generic;

namespace OpenSpark.Shared.Events
{
    public class MultiPayloadEvent
    {
        public Guid MultiQueryId { get; set; }
        public IList<IPayloadEvent> Payloads { get; set; }
    }
}