using System;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Events.Sagas;

namespace OpenSpark.ApiGateway.Actors
{
    /// <summary>
    /// A callback actor for other actor systems to contact with results when ready.
    /// </summary>
    public class CallbackActor : ReceiveActor
    {
        public CallbackActor(IEventEmitterService eventEmitter)
        {
            var subscriptions = ImmutableDictionary<Guid, IActorRef>.Empty;

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
                subscriptions = subscriptions.Add(transactionId, subscriptionActor);
                Sender.Tell(true);
            });

            Receive<Terminated>(terminated =>
            {
                Context.Unwatch(terminated.ActorRef);

                subscriptions = subscriptions.Where(v => 
                    !v.Value.Equals(terminated.ActorRef)).ToImmutableDictionary();
            });

            // Client wants to subscribe
            Receive<SubscribeToSagaTransactionCommand>(command =>
            {
                if (!subscriptions.ContainsKey(command.TransactionId))
                {
                    Console.WriteLine($"Failed to subscribe to transaction: Unable to find subscription for transaction: {command.TransactionId}");
                    return;
                }

                subscriptions[command.TransactionId].Tell(command);
            });

            // Saga emitted an event for the client
            Receive<SagaFinishedEvent>(@event =>
            {
                if (!subscriptions.ContainsKey(@event.TransactionId))
                {
                    Console.WriteLine($"Failed to send saga message to client: Unable to find subscription for transaction: {@event.TransactionId}");
                    return;
                }

                subscriptions[@event.TransactionId].Tell(@event);
            });
        }
    }
}