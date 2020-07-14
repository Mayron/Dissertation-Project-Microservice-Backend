using System;
using Akka.Actor;
using OpenSpark.ApiGateway.Models;
using OpenSpark.ApiGateway.Models.StateData;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Events.Sagas;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
    // TODO: This can benefit from Akka.Persistence to recover from errors

    public class AddPostSagaActor : FSM<AddPostSagaActor.SagaState, ISagaStateData>
    {
        private readonly IActorSystemService _actorSystemService;
        private readonly IEventEmitterService _eventEmitter;

        public enum SagaState
        {
            Idle,
            VerifyingUser,
            AddingPost,
            Success,
            Error
        }

        public AddPostSagaActor(IActorSystemService actorSystemService, IEventEmitterService eventEmitter)
        {
            _actorSystemService = actorSystemService;
            _eventEmitter = eventEmitter;

            StartWith(SagaState.Idle, Uninitialized.Instance);

            When(SagaState.Idle, HandleIdleEvents);
            When(SagaState.VerifyingUser, HandleVerifyingUserEvents, TimeSpan.FromSeconds(5));
            When(SagaState.AddingPost, HandleAddingPostEvents, TimeSpan.FromSeconds(5));

            // unhandled messages - Not sure this will ever be hit!
            WhenUnhandled(ev => GoTo(SagaState.Error));

            When(SagaState.Success, ev =>
            {
                if (ev.StateData is SuccessStateData data)
                {
                    _eventEmitter.BroadcastToGroup(data.GroupId, "PostAdded", data.AddedPost);
                }

                return Stop();
            });

            When(SagaState.Error, ev =>
            {
                // TODO: Remove the Success and Error sage states and only check for WhenUnhandled
                // TODO: Then, check if we receive an ErroSagaEvent (new object) for a custom message
                // TODO: Do not update the state - we need the current one. Then analyse if we need to rollback anything!



                // TODO: Need to use compensating transaction
                //                var error = ev.StateData as ErrorStateData;
                //                Console.WriteLine($"Failed: {error?.Message}");
                //StateData.TransactionId, "Received unknown message for event: " + ev.FsmEvent
                if (ev.StateData is ErrorStateData data)
                {
                    var previousWorkingStateData = data.PreviousStateData;
                    Console.WriteLine(previousWorkingStateData);
                    // In case we want a custom error message we can wrap the old state data in the ErrorStateData
                }
                else
                {
                    // We didn't receive a new message...
                }

                return Stop();
            });
        }

        public FSMBase.State<SagaState, ISagaStateData> HandleIdleEvents(Event<ISagaStateData> ev)
        {
            if (ev.FsmEvent is CreateAddPostRequestCommand command && ev.StateData is Uninitialized)
            {
                _actorSystemService.SendGroupsCommand(new VerifyUserPostRequestCommand
                {
                    TransactionId = command.TransactionId,
                    UserId = command.User.UserId,
                    GroupId = command.GroupId,
                });

                return GoTo(SagaState.VerifyingUser).Using(new VerifyingUserStateData(command.TransactionId));
            }

            return null;
        }

        private FSMBase.State<SagaState, ISagaStateData> HandleVerifyingUserEvents(Event<ISagaStateData> fsmEvent)
        {
            switch (fsmEvent.FsmEvent)
            {
                case UserVerifiedEvent @event when fsmEvent.StateData is VerifyingUserStateData data:
                    if (@event.TransactionId != data.TransactionId)
                    {
                        Console.WriteLine($"Unexpected transaction Id for UserVerifiedEvent: {@event.TransactionId}");
                        return Stay();
                    }

                    _actorSystemService.SendDiscussionsCommand(new AddPostCommand
                    {
                        TransactionId = data.TransactionId,
                    });

                    return GoTo(SagaState.AddingPost).Using(new AddingPostStateData(data.TransactionId));

                case UserVerificationFailedEvent @event when fsmEvent.StateData is VerifyingUserStateData data:
                    if (@event.TransactionId != data.TransactionId)
                    {
                        Console.WriteLine($"Unexpected transaction Id for UserVerificationFailedEvent: {@event.TransactionId}");
                        return Stay();
                    }

                    return GoTo(SagaState.Error).Using(
                        new ErrorStateData(StateData, "User does not have permission to post to group"));

                case StateTimeout _:
                    return GoTo(SagaState.Error).Using(
                        new ErrorStateData(StateData, "Request timed out while verifying user."));
            }

            return null;
        }

        private FSMBase.State<SagaState, ISagaStateData> HandleAddingPostEvents(Event<ISagaStateData> fsmEvent)
        {
            switch (fsmEvent.FsmEvent)
            {
                case PostAddedEvent @event when fsmEvent.StateData is AddingPostStateData data:
                    if (@event.TransactionId != data.TransactionId)
                    {
                        Console.WriteLine($"Unexpected transaction Id for PostAddedEvent: {@event.TransactionId}");
                        return Stay();
                    }
                    
                    return GoTo(SagaState.Success).Using(new SuccessStateData(data.TransactionId, @event.Post, @event.GroupId));

                case StateTimeout _:
                    return GoTo(SagaState.Error).Using(
                        new ErrorStateData(StateData, "Request timed out while adding post."));
            }

            return null;
        }
    }
}
