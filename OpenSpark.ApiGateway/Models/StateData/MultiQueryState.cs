using System;
using Akka.Actor;
using OpenSpark.Shared.Events.Payloads;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OpenSpark.ApiGateway.Models.StateData
{
    public class MultiQueryState
    {
        public IImmutableDictionary<Guid, IPayloadEvent> Received { get; }
        public IImmutableDictionary<Guid, int> Pending { get; }

        public MultiQueryState(
            IImmutableDictionary<Guid, IPayloadEvent> received,
            IImmutableDictionary<Guid, int> pending)
        {
            Received = received;
            Pending = pending;
        }

        public MultiQueryState(IDictionary<Guid, int> pending)
        {
            Received = ImmutableDictionary<Guid, IPayloadEvent>.Empty;
            Pending = pending.ToImmutableDictionary();
        }
    }
}