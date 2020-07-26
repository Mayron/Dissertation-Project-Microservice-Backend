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
        private readonly IFirestoreService _firestoreService;

        public MultiQueryManagerActor(
            IActorSystemService actorSystemService, 
            IActorRef callbackActor,
            IFirestoreService firestoreService)
        {
            _actorSystemService = actorSystemService;
            _callbackActor = callbackActor;
            _firestoreService = firestoreService;

            Receive<MultiQueryContext>(multiQuery =>
            {
                var target = CreateAggregator(multiQuery);
                CreateHandler(multiQuery, target);
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

                nameof(GetPostsAggregator) =>
                Context.ActorOf(Props.Create(() => new GetPostsAggregator(multiQueryContext, _callbackActor)), actorName),

                _ => throw new Exception($"Failed to find aggregator: {multiQueryContext.Aggregator}"),
            };
        }

        private void CreateHandler(MultiQueryContext context, IActorRef aggregator)
        {
            var actorName = $"{context.Handler}-{context.Id}";

            // creating the handler will auto trigger queries to fire
            switch (context.Handler)
            {
                case nameof(MultiQueryParallelHandler):
                    Context.ActorOf(Props.Create(() =>
                        new MultiQueryParallelHandler(context, aggregator, _actorSystemService)), actorName);
                    break;

                case nameof(GetPostsMultiQueryHandler):
                    Context.ActorOf(Props.Create(() =>
                            new GetPostsMultiQueryHandler(context, aggregator, _actorSystemService, _firestoreService)), actorName);
                    break;

                default:
                    throw new Exception($"Failed to find handler: {context.Handler}");
            }
        }
    }
}