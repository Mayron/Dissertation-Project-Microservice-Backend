using System;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;
using OpenSpark.ApiGateway.StateData;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;

namespace OpenSpark.ApiGateway.Actors.MultiQueryHandlers
{
    public class MultiQueryParallelHandler : UntypedActor
    {
        private readonly ICancelable _queryTimeoutTimer;
        private readonly IImmutableList<QueryContext> _queries;
        private readonly IActorRef _aggregator;
        private readonly IActorSystemService _actorSystemService;
        private readonly Guid _multiQueryId;

        private sealed class MultiQueryTimeout
        {
            public static MultiQueryTimeout Instance { get; } = new MultiQueryTimeout();

            private MultiQueryTimeout() {}
        }

        public MultiQueryParallelHandler(MultiQueryContext multiQueryContext, IActorRef aggregator, IActorSystemService actorSystemService)
        {
            _multiQueryId = multiQueryContext.Id;

            _queryTimeoutTimer = Context.System.Scheduler
                .ScheduleTellOnceCancelable(multiQueryContext.TimeoutInSeconds, Self, MultiQueryTimeout.Instance, Self);

            _queries = multiQueryContext.Queries.ToImmutableList();
            _aggregator = aggregator;
            _actorSystemService = actorSystemService;

            var pending = multiQueryContext.Queries.ToDictionary(
                queryContext => queryContext.Query.MetaData.QueryId,
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
                    case PayloadEvent @event when @event.MetaData.MultiQueryId == _multiQueryId:
                        ReceivedEvent(@event.MetaData.QueryId, @event, state);
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
                // no more pending states - tell the multi-query target actor we're finished
                _aggregator.Tell(new MultiPayloadEvent
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
                _actorSystemService.SendRemoteQuery(queryContext, Self);
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