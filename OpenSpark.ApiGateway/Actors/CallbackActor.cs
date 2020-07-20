using System;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Events.Sagas;

namespace OpenSpark.ApiGateway.Actors
{
    /// <summary>
    /// A callback actor for other actor systems to contact with results when ready.
    /// </summary>
    public class CallbackActor : ReceiveActor
    {
        private IImmutableDictionary<Guid, IActorRef> _subscriptions;

        public CallbackActor(IEventEmitterService eventEmitter)
        {
            _subscriptions = ImmutableDictionary<Guid, IActorRef>.Empty;

            Receive<PayloadEvent>(@event =>
            {
                var (connectionId, callback, eventData) = @event;
                eventEmitter.BroadcastToClient(connectionId, callback, eventData);
            });

            // Register
            Receive<Guid>(transactionId =>
            {
                var subscriptionActor = Context.ActorOf(Props.Create(() =>
                        new SagaSubscriptionActor(eventEmitter, transactionId)),
                    $"SagaSubscription-{transactionId}");

                Context.Watch(subscriptionActor);
                _subscriptions = _subscriptions.Add(transactionId, subscriptionActor);
            });

            Receive<Terminated>(terminated =>
            {
                Context.Unwatch(terminated.ActorRef);

                _subscriptions = _subscriptions.Where(v => 
                    !v.Value.Equals(terminated.ActorRef)).ToImmutableDictionary();
            });

            // Client wants to subscribe
            Receive<SubscribeToSagaTransactionCommand>(command =>
            {
                if (!_subscriptions.ContainsKey(command.TransactionId))
                {
                    Console.WriteLine($"Failed to subscribe to transaction: Unable to find subscription for transaction: {command.TransactionId}");
                    return;
                }

                _subscriptions[command.TransactionId].Tell(command);
            });

            // Saga emitted an event for the client
            Receive<SagaFinishedEvent>(@event =>
            {
                if (!_subscriptions.ContainsKey(@event.TransactionId))
                {
                    Console.WriteLine($"Failed to send saga message to client: Unable to find subscription for transaction: {@event.TransactionId}");
                    return;
                }

                _subscriptions[@event.TransactionId].Tell(@event);
            });
        }
    }
}