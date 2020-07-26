using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using Newtonsoft.Json;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;
using OpenSpark.ApiGateway.StateData;
using OpenSpark.Domain;
using OpenSpark.Shared.Events.Payloads;

namespace OpenSpark.ApiGateway.Actors.MultiQueryHandlers
{
    public class MultiQueryParallelHandler : FSM<string, MultiQueryStateData>
    {
        protected IActorSystemService ActorSystemService { get; }
        protected Guid MultiQueryId { get; }
        protected User User { get; }

        protected const string InitialState = "InitialState";

        private readonly IActorRef _aggregator;
        private readonly ICancelable _queryTimeoutTimer;
        private readonly Stack<string> _states = new Stack<string>();

        // This is only used for PreStart to fire off initial queries
        private readonly IImmutableList<QueryContext> _preStartQueries;

        private sealed class MultiQueryTimeout
        {
            public static MultiQueryTimeout Instance { get; } = new MultiQueryTimeout();

            private MultiQueryTimeout()
            {
            }
        }

        public MultiQueryParallelHandler(
            MultiQueryContext context, 
            IActorRef aggregator, 
            IActorSystemService actorSystemService)
        {
            MultiQueryId = context.Id;
            User = context.User;

            _queryTimeoutTimer = Context.System.Scheduler
                .ScheduleTellOnceCancelable(
                    context.TimeoutInSeconds * 1000,
                    Self, MultiQueryTimeout.Instance, Self);

            _preStartQueries = context.Queries.ToImmutableList();
            _aggregator = aggregator;
            ActorSystemService = actorSystemService;

            var pending = GetPendingQueries(context.Queries);

            StartWith(InitialState, new MultiQueryStateData(pending));
            When(InitialState, WaitingForReplies);
        }

        private State<string, MultiQueryStateData> WaitingForReplies(Event<MultiQueryStateData> fsmEvent)
        {
            switch (fsmEvent.FsmEvent)
            {
                case PayloadEvent @event when @event.MetaData.MultiQueryId == MultiQueryId:
                    return ReceivedEvent(@event.MetaData.QueryId, @event);

                case MultiQueryTimeout _:
                    Console.WriteLine("Multi-Query timed out");
                    Context.Stop(Self);
                    break;

                case PayloadEvent @event:
                    Console.WriteLine($"Invalid MultiQueryId for PayloadEvent. Expected {MultiQueryId}, but received {@event.MetaData.MultiQueryId}");
                    break;

                default:
                    Console.WriteLine($"Unknown message type: {fsmEvent.FsmEvent.GetType().Name} - {JsonConvert.SerializeObject(fsmEvent.FsmEvent)}");
                    break;
            }

            return null;
        }

        private State<string, MultiQueryStateData> ReceivedEvent(Guid queryId, IPayloadEvent @event)
        {
            var nextPendingState = StateData.Pending.Remove(queryId);
            var nextReceivedState = StateData.Received.Add(queryId, @event);

            if (nextPendingState.Count != 0)
                return Stay().Using(new MultiQueryStateData(nextReceivedState, nextPendingState));

            // no more pending states - move to next state or stop if no next state
            if (_states.Count > 0)
                return GoTo(_states.Pop()).Using(new MultiQueryStateData(nextReceivedState));

            _aggregator.Tell(new MultiPayloadEvent
            {
                MultiQueryId = MultiQueryId,
                Payloads = nextReceivedState.Values.ToList()
            });

            return Stop();
        }

        protected override void PreStart()
        {
            foreach (var queryContext in _preStartQueries)
                ActorSystemService.SendRemoteQuery(queryContext, Self);
        }

        protected override void PostStop()
        {
            _queryTimeoutTimer.Cancel();
        }

        protected static IImmutableDictionary<Guid, int> GetPendingQueries(IEnumerable<QueryContext> queries) =>
            queries.ToImmutableDictionary(
                queryContext => queryContext.Query.MetaData.QueryId,
                queryContext => queryContext.RemoteSystemId);

        protected void SetNextState(string nextState)
        {
            When(nextState, WaitingForReplies);
            _states.Push(nextState);
        }
        
    }
}