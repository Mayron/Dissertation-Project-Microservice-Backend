using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Events.Sagas;
using System;
using System.Collections.Immutable;

namespace OpenSpark.ApiGateway.Actors
{
    /// <summary>
    /// A callback actor for other actor systems to contact with results when ready.
    /// </summary>
    public class CallbackActor : ReceiveActor
    {
        private readonly IEventEmitterService _eventEmitter;
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
            _eventEmitter = eventEmitter;
            _subscriptions = ImmutableDictionary<Guid, ClientSubscription>.Empty;

            Receive<PayloadEvent>(@event =>
            {
                var (connectionId, callback, eventData) = @event;
                _eventEmitter.BroadcastToClient(connectionId, callback, eventData);
            });

            Receive<SubscribeToSagaTransactionCommand>(command =>
            {
                _subscriptions = _subscriptions.Add(command.TransactionId,
                    new ClientSubscription(command.Callback, command.ConnectionId));
            });

            Receive<SagaMessageEmittedEvent>(@event =>
            {
                if (!_subscriptions.ContainsKey(@event.TransactionId)) return;

                var sub = _subscriptions[@event.TransactionId];

                _eventEmitter.BroadcastToClient(sub.ConnectionId, sub.ClientCallback, @event);
                _subscriptions = _subscriptions.Remove(@event.TransactionId);
            });
        }
    }
}