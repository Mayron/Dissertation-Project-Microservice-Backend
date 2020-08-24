using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using Newtonsoft.Json;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;
using OpenSpark.ApiGateway.StateData;
using OpenSpark.Shared.Domain;
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
        private bool _timedOut;

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
            IActorSystemService actorSystem)
        {
            MultiQueryId = context.Id;
            User = context.User;

            _queryTimeoutTimer = Context.System.Scheduler
                .ScheduleTellOnceCancelable(
                    context.TimeoutInSeconds * 1000,
                    Self, MultiQueryTimeout.Instance, Self);

            _preStartQueries = context.Queries.ToImmutableList();
            _aggregator = aggregator;
            ActorSystemService = actorSystem;

            var pending = GetPendingQueries(context.Queries);

            StartWith(InitialState, new MultiQueryStateData(pending));
            When(InitialState, WaitingForReplies);
        }

        private State<string, MultiQueryStateData> WaitingForReplies(Event<MultiQueryStateData> fsmEvent)
        {
            switch (fsmEvent.FsmEvent)
            {
                case PayloadEvent @event when @event.MetaData.ParentId == MultiQueryId:
                    return ReceivedEvent(@event.MetaData.Id, @event);

                case MultiQueryTimeout _:
                    Console.WriteLine("Multi-Query timed out");
                    _timedOut = true;
                    return Stop();

                case PayloadEvent @event:
                    Console.WriteLine($"Invalid MultiQueryId for PayloadEvent. Expected {MultiQueryId}, but received {@event.MetaData.ParentId}");
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

            var nextStateData = new MultiQueryStateData(nextReceivedState);

            // no more pending states - move to next state or stop if no next state
            return _states.Count > 0 
                ? GoTo(_states.Pop()).Using(nextStateData) 
                : Stop().Using(nextStateData);
        }

        protected override void PreStart()
        {
            foreach (var queryContext in _preStartQueries)
                ActorSystemService.SendQuery(queryContext, Self);

            base.PreStart();
        }

        protected override void PostStop()
        {
            _queryTimeoutTimer.Cancel();

            if (!_timedOut)
            {
                _aggregator.Tell(new MultiPayloadEvent
                {
                    MultiQueryId = MultiQueryId,
                    Payloads = StateData.Received.Values.ToList()
                });
            }

            base.PostStop();
        }

        protected static IImmutableDictionary<Guid, int> GetPendingQueries(IEnumerable<QueryContext> queries) =>
            queries.ToImmutableDictionary(
                queryContext => queryContext.Query.MetaData.Id,
                queryContext => queryContext.RemoteSystemId);

        protected void SetNextState(string nextState)
        {
            When(nextState, WaitingForReplies);
            _states.Push(nextState);
        }
        
    }
}