using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Events.Sagas;
using System;

namespace OpenSpark.ApiGateway.Actors
{
    /// <summary>
    /// A callback actor for other actor systems to contact with results when ready.
    /// </summary>
    public class SagaSubscriptionActor : ReceiveActor
    {
        private class ClientSubscription
        {
            public string ClientCallback { get; set; }
            public string ConnectionId { get; set; }
        }

        private readonly IEventEmitterService _eventEmitter;
        private ClientSubscription _subscription;
        private SagaFinishedEvent _message;

        public SagaSubscriptionActor(IEventEmitterService eventEmitter, Guid transactionId)
        {
            _eventEmitter = eventEmitter;

            Receive<SubscribeToSagaTransactionCommand>(command =>
            {
                if (_message != null)
                {
                    // Message arrived quickly and is already available.
                    Broadcast(command.ConnectionId, command.Callback);
                    return;
                }

                // Wait for the message to be delivered before sending to client.
                _subscription = new ClientSubscription
                {
                    ClientCallback = command.Callback,
                    ConnectionId = command.ConnectionId
                };
            });

            Receive<SagaFinishedEvent>(@event =>
            {
                if (@event.TransactionId != transactionId)
                {
                    Console.WriteLine($"Unknown transaction Id {@event.TransactionId}. Expected: {transactionId}");
                    return;
                }

                _message = @event;

                if (_subscription != null)
                {
                    // Client has already subscribed and message can be delivered safely.
                    Broadcast(_subscription.ConnectionId, _subscription.ClientCallback);
                }
            });
        }

        private void Broadcast(string connectionId, string callback)
        {
            _eventEmitter.BroadcastToClient(connectionId, callback, _message);
            Self.GracefulStop(TimeSpan.FromSeconds(5));
        }
    }
}