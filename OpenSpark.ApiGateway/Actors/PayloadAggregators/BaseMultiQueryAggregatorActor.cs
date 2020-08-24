using Akka.Actor;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.Shared;
using OpenSpark.Shared.Events.Payloads;
using System;
using System.Collections.Generic;

namespace OpenSpark.ApiGateway.Actors.PayloadAggregators
{
    public abstract class BaseMultiQueryAggregatorActor : ReceiveActor
    {
        private readonly MultiQueryContext _context;

        protected BaseMultiQueryAggregatorActor(MultiQueryContext context, IActorRef callback)
        {
            _context = context;

            SetReceiveTimeout(TimeSpan.FromSeconds(context.TimeoutInSeconds));

            Receive<MultiPayloadEvent>(@event =>
            {
                if (@event.MultiQueryId != _context.Id)
                    throw new Exception($"Invalid multi query Id. Expected {_context.Id} but got {@event.MultiQueryId}");

                // Call the abstract template method
                var payload = AggregatePayload(@event.Payloads);
                callback.Tell(payload);

                Context.Stop(Self);
            });

            Receive<ReceiveTimeout>(r =>
            {
                callback.Tell(new PayloadEvent
                {
                    MetaData = new MetaData
                    {
                        ConnectionId = _context.ConnectionId,
                        Callback = _context.Callback,
                    },
                    Errors = new[] { "Request timed out" }
                });

                Context.Stop(Self);
            });
        }

        private PayloadEvent AggregatePayload(IEnumerable<IPayloadEvent> payloads)
        {
            var successful = new List<object>();
            var errors = new List<string>();

            foreach (var @event in payloads) // not sure what to do with key yet
            {
                switch (@event)
                {
                    case PayloadEvent payloadEvent when payloadEvent.Payload != null:
                        successful.Add(payloadEvent.Payload);
                        break;

                    case PayloadEvent errorEvent when errorEvent.Errors != null:
                        errors.AddRange(errorEvent.Errors);
                        break;
                }
            }

            if (errors.Count > 0)
            {
                return new PayloadEvent
                {
                    MetaData = new MetaData
                    {
                        ConnectionId = _context.ConnectionId,
                        Callback = _context.Callback,
                    },
                    Errors = errors.ToArray()
                };
            }

            var results = GetAggregatedPayload(successful);

            return new PayloadEvent
            {
                MetaData = new MetaData
                {
                    ConnectionId = _context.ConnectionId,
                    Callback = _context.Callback,
                },
                Payload = results
            };
        }

        protected abstract object GetAggregatedPayload(List<object> payloads);
    }
}