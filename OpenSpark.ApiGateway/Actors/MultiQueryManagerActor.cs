using System;
using System.Collections.Immutable;
using Akka.Actor;
using OpenSpark.ApiGateway.Actors.MultiQueries;
using OpenSpark.ApiGateway.Models;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Events;
using OpenSpark.Shared.Queries;

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
                var target = CreateMultiQueryActor(multiQuery);

                // creating this will auto trigger queries to fire
                Context.ActorOf(Props.Create(() => 
                        new MultiQueryHandlerActor(multiQuery, target, actorSystemService)), 
                    $"MultiQueryHandler-{multiQuery.Id}");
            });

            Receive<MultiPayloadEvent>(@event => { });
        }

        private IActorRef CreateMultiQueryActor(MultiQuery query)
        {
            var actorName = $"{query.MultiQueryName}-{query.Id}";

            return query.MultiQueryName switch
            {
                nameof(GroupConnectsListMultiQueryActor) =>
                Context.ActorOf(
                    Props.Create(() => new GroupConnectsListMultiQueryActor(query, _callbackActor)), actorName),

                _ => throw new Exception($"Failed to find MultiQueryActor: {query.MultiQueryName}"),
            };
        }
    }
}
