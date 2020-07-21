using Akka.Actor;
using OpenSpark.ApiGateway.Models.StateData;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Events;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace OpenSpark.ApiGateway.Actors
{
    public class MultiQueryHandlerActor : UntypedActor
    {
        private readonly ICancelable _queryTimeoutTimer;
        private readonly IImmutableList<QueryContext> _queries;
        private readonly IActorRef _target;
        private readonly IActorSystemService _actorSystemService;
        private readonly Guid _multiQueryId;

        private sealed class MultiQueryTimeout
        {
            public static MultiQueryTimeout Instance { get; } = new MultiQueryTimeout();
            private MultiQueryTimeout() {}
        }

        public MultiQueryHandlerActor(MultiQuery multiQuery, IActorRef target, IActorSystemService actorSystemService)
        {
            _multiQueryId = multiQuery.Id;

            _queryTimeoutTimer = Context.System.Scheduler
                .ScheduleTellOnceCancelable(multiQuery.TimeOutInSeconds, Self, MultiQueryTimeout.Instance, Self);

            _queries = multiQuery.Queries.ToImmutableList();
            _target = target;
            _actorSystemService = actorSystemService;

            var pending = multiQuery.Queries.ToDictionary(
                queryContext => queryContext.Query.Id,
                queryContext => queryContext.RemoteSystemId);

            Become(WaitingForReplies(new MultiQueryState(pending)));
        }

        private UntypedReceive WaitingForReplies(MultiQueryState state)
        {
            return message =>
            {
                Console.WriteLine($"Message reply received: {message}");

                switch (message)
                {
                    case PayloadEvent @event when @event.MultiQueryId == _multiQueryId:
                        ReceivedEvent(@event.QueryId, @event, state);
                        break;

                    case MultiQueryTimeout _:
                        Console.WriteLine("Multi-Query timed out");
                        Context.Stop(Self);
                        break;
                    default:
                        throw new Exception($"Unknown message type: {message}");
                }
            };
        }

        private void ReceivedEvent(Guid queryId, IPayloadEvent @event, MultiQueryState state)
        {
            var nextPendingState = state.Pending.Remove(queryId);
            var nextReceivedState = state.Received.Add(queryId, @event);

            if (nextPendingState.Count == 0)
            {
                // no more pending states - tell the target we're finished
                _target.Tell(new MultiPayloadEvent
                {
                    MultiQueryId = _multiQueryId,
                    Payloads = nextReceivedState.Values.ToList()
                });

                Context.Stop(Self);
                return;
            }

            // Move to next state
            Become(WaitingForReplies(new MultiQueryState(nextReceivedState, nextPendingState)));
        }

        protected override void PreStart()
        {
            foreach (var queryContext in _queries)
                _actorSystemService.SendRemoteMessage(queryContext.RemoteSystemId, queryContext.Query, Self);
        }

        protected override void PostStop()
        {
            _queryTimeoutTimer.Cancel();
        }

        protected override void OnReceive(object message)
        {
            // Block all messages until actor has started and moved to initial state
        }
    }
}