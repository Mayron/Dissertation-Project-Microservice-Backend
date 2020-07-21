using Akka.Actor;
using OpenSpark.ApiGateway.Actors.MultiQueries;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Events;
using OpenSpark.Shared.Queries;
using System;

namespace OpenSpark.ApiGateway.Actors
{
    public class MultiQueryManagerActor : ReceiveActor
    {
        private readonly IActorRef _callbackActor;

        public MultiQueryManagerActor(IActorSystemService actorSystemService, IActorRef callbackActor)
        {
            _callbackActor = callbackActor;

            Receive<MultiQuery>(multiQuery =>
            {
                multiQuery.Id = Guid.NewGuid();
                var target = CreateMultiQueryActor(multiQuery);

                // creating this will auto trigger queries to fire
                Context.ActorOf(Props.Create(() =>
                        new MultiQueryHandlerActor(multiQuery, target, actorSystemService)),
                    $"MultiQueryHandler-{multiQuery.Id}");
            });

            Receive<MultiPayloadEvent>(@event => { });
        }

        private IActorRef CreateMultiQueryActor(MultiQuery multiQuery)
        {
            var actorName = $"{multiQuery.MultiQueryName}-{multiQuery.Id}";

            foreach (var queryContext in multiQuery.Queries)
            {
                var query = queryContext.Query;
                query.Id = Guid.NewGuid();
                query.MultiQueryId = multiQuery.Id;
            }

            return multiQuery.MultiQueryName switch
            {
                nameof(GroupConnectsListMultiQueryActor) =>
                Context.ActorOf(
                    Props.Create(() => new GroupConnectsListMultiQueryActor(multiQuery, _callbackActor)), actorName),

                _ => throw new Exception($"Failed to find MultiQueryActor: {multiQuery.MultiQueryName}"),
            };
        }
    }
}