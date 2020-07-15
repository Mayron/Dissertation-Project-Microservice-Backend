using System;
using System.Collections.Immutable;
using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Events;
using OpenSpark.Shared.Events.Sagas;

namespace OpenSpark.ApiGateway.Actors
{
    /// <summary>
    /// A callback actor for other actor systems to contact with results when ready.
    /// </summary>
    public class CallbackActor : ReceiveActor
    {
        private IImmutableDictionary<Guid, ClientSubscription> _subscriptions;

        private class ClientSubscription
        {
            public string ClientCallback { get; }
            public string ConnectionId { get; }

            public ClientSubscription(string clientCallback, string connectionId)
            {
                ClientCallback = clientCallback;
                ConnectionId = connectionId;
            }
        }

        public CallbackActor(IEventEmitterService eventEmitter)
        {
            _subscriptions = ImmutableDictionary<Guid, ClientSubscription>.Empty;

            Receive<NewsFeedReceivedEvent>(@event =>
            {
                eventEmitter.BroadcastToClient(@event.ConnectionId, "NewsFeedUpdate", @event.Posts);
            });

            Receive<SubscribeToSagaTransactionCommand>(command =>
            {
                _subscriptions = _subscriptions.Add(command.TransactionId, new ClientSubscription(command.Callback, command.ConnectionId));
            });

            Receive<SagaFinishedEvent>(@event =>
            {
                if (_subscriptions.ContainsKey(@event.TransactionId))
                {
                    var clientSubscription = _subscriptions[@event.TransactionId];
                    eventEmitter.BroadcastToClient(clientSubscription.ConnectionId, clientSubscription.ClientCallback, @event);
                    _subscriptions = _subscriptions.Remove(@event.TransactionId);
                }
            });
        }
    }
}
