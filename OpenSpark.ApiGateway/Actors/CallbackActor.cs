using Akka.Actor;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Events.Sagas;
using System;
using System.Collections.Generic;

namespace OpenSpark.ApiGateway.Actors
{
    /// <summary>
    /// A callback actor for other actor systems to contact with results when ready.
    /// </summary>
    public class CallbackActor : ReceiveActor
    {
        public CallbackActor(IEventEmitterService eventEmitter)
        {
            var subscriptions = new Dictionary<Guid, IActorRef>();

            Receive<PayloadEvent>(eventEmitter.BroadcastToClient);

            // Register Saga transaction
            Receive<SagaContext>(sagaContext =>
            {
                var subscriptionActor = Context.ActorOf(Props.Create(() =>
                        new SagaSubscriptionActor(eventEmitter, sagaContext.TransactionId)),
                    $"SagaSubscription-{sagaContext.TransactionId}");

                Context.Watch(subscriptionActor);
                subscriptions.Add(sagaContext.TransactionId, subscriptionActor);
                Sender.Tell(true);
            });

            Receive<Terminated>(terminated =>
            {
                Context.Unwatch(terminated.ActorRef);
                subscriptions.RemoveAll(v => !v.Value.Equals(terminated.ActorRef));
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