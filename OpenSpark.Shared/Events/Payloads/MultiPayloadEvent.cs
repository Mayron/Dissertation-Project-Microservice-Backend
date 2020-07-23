using System;
using System.Collections.Generic;

namespace OpenSpark.Shared.Events.Payloads
{
    public class MultiPayloadEvent
    {
        public Guid MultiQueryId { get; set; }
        public IList<IPayloadEvent> Payloads { get; set; }
    }
}