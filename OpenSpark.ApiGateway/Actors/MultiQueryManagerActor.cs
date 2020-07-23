using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using System;
using OpenSpark.ApiGateway.Actors.PayloadAggregators;

namespace OpenSpark.ApiGateway.Actors
{
    public class MultiQueryManagerActor : ReceiveActor
    {
        private readonly IActorSystemService _actorSystemService;
        private readonly IActorRef _callbackActor;

        public MultiQueryManagerActor(IActorSystemService actorSystemService, IActorRef callbackActor)
        {
            _actorSystemService = actorSystemService;
            _callbackActor = callbackActor;

            Receive<MultiQuery>(multiQuery =>
            {
                multiQuery.Id = Guid.NewGuid();

                foreach (var queryContext in multiQuery.Queries)
                {
                    var query = queryContext.Query;
                    query.Id = Guid.NewGuid();
                    query.MultiQueryId = multiQuery.Id;
                }

                var target = CreateAggregator(multiQuery);
                var handler = CreateHandler(multiQuery, target);
            });

            Receive<MultiPayloadEvent>(@event => { });
        }

        private IActorRef CreateAggregator(MultiQuery multiQuery)
        {
            var actorName = $"{multiQuery.Aggregator}-{multiQuery.Id}";

            return multiQuery.Aggregator switch
            {
                nameof(GroupConnectionsListAggregatorActor) =>
                Context.ActorOf(
                    Props.Create(() => new GroupConnectionsListAggregatorActor(multiQuery, _callbackActor)), actorName),

                _ => throw new Exception($"Failed to find aggregator: {multiQuery.Aggregator}"),
            };
        }

        private IActorRef CreateHandler(MultiQuery multiQuery, IActorRef aggregator)
        {
            var actorName = $"{multiQuery.Handler}-{multiQuery.Id}";

            return multiQuery.Handler switch
            {
                nameof(MultiQueryParallelHandlerActor) =>
                // creating this will auto trigger queries to fire
                Context.ActorOf(Props.Create(() =>
                        new MultiQueryParallelHandlerActor(multiQuery, aggregator, _actorSystemService)), actorName),

                _ => throw new Exception($"Failed to find handler: {multiQuery.Aggregator}"),
            };
        }
    }
}