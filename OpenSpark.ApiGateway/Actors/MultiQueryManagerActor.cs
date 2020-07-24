using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using System;
using OpenSpark.ApiGateway.Actors.MultiQueryHandlers;
using OpenSpark.ApiGateway.Actors.PayloadAggregators;
using OpenSpark.ApiGateway.Builders;

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

            Receive<MultiQueryContext>(multiQuery =>
            {
                multiQuery.Id = Guid.NewGuid();

                var target = CreateAggregator(multiQuery);
                var handler = CreateHandler(multiQuery, target);
            });

            Receive<MultiPayloadEvent>(@event => { });
        }

        private IActorRef CreateAggregator(MultiQueryContext multiQueryContext)
        {
            var actorName = $"{multiQueryContext.Aggregator}-{multiQueryContext.Id}";

            return multiQueryContext.Aggregator switch
            {
                nameof(GroupConnectionsListAggregator) =>
                Context.ActorOf(
                    Props.Create(() => new GroupConnectionsListAggregator(multiQueryContext, _callbackActor)), actorName),

                _ => throw new Exception($"Failed to find aggregator: {multiQueryContext.Aggregator}"),
            };
        }

        private IActorRef CreateHandler(MultiQueryContext multiQueryContext, IActorRef aggregator)
        {
            var actorName = $"{multiQueryContext.Handler}-{multiQueryContext.Id}";

            return multiQueryContext.Handler switch
            {
                nameof(MultiQueryParallelHandler) =>
                // creating this will auto trigger queries to fire
                Context.ActorOf(Props.Create(() =>
                        new MultiQueryParallelHandler(multiQueryContext, aggregator, _actorSystemService)), actorName),

                _ => throw new Exception($"Failed to find handler: {multiQueryContext.Aggregator}"),
            };
        }
    }
}