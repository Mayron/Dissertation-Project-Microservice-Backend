using System;
using Akka.Actor;
using OpenSpark.ApiGateway.Models;
using OpenSpark.ApiGateway.Models.StateData;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Events.Sagas;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
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

            When(SagaState.Idle, HandleIdleEvent);
            When(SagaState.VerifyingUser, HandleVerifyingUserEvent, TimeSpan.FromSeconds(5));

            When(SagaState.AddingPost, HandleAddingPostEvent, TimeSpan.FromSeconds(5)); // generates StateTimeout message if no response in 5 seconds

            // unhandled state
            WhenUnhandled(ev => GoTo(SagaState.Error)
                .Using(new ErrorStateData(ev.StateData.TransactionId, "Received unknown state for event: " + ev.FsmEvent)));

            When(SagaState.Success, ev =>
            {
                var success = ev.StateData as SuccessStateData;
                Console.WriteLine($"Success: {success?.Message}");
                return Stop();
            });

            When(SagaState.Error, ev =>
            {
                var error = ev.StateData as ErrorStateData;
                Console.WriteLine($"Failed: {error?.Message}");
                return Stop();
            });
        }

        private FSMBase.State<SagaState, ISagaStateData> HandleAddingPostEvent(Event<ISagaStateData> fsmEvent)
        {
            switch (fsmEvent.FsmEvent)
            {
                case PostAddedEvent @event when fsmEvent.StateData is AddingPostStateData data:
                    if (@event.TransactionId != data.TransactionId)
                    {
                        Console.WriteLine($"Unexpected transaction Id for PostAddedEvent: {@event.TransactionId}");
                        return Stay();
                    }

                    _eventEmitter.ReceivedEvent(@event);
                    return GoTo(SagaState.Success).Using(new SuccessStateData(data.TransactionId, "Successfully added post"));

                case StateTimeout _:
                    return GoTo(SagaState.Error).Using(
                        new ErrorStateData(fsmEvent.StateData.TransactionId, "Request timed out while adding post."));

                default:
                    GoTo(SagaState.Error).Using(
                        new ErrorStateData(fsmEvent.StateData.TransactionId, $"Received unknown event state during AddingPost state: {fsmEvent.FsmEvent}"));
                    break;
            }

            return null;
        }

        private FSMBase.State<SagaState, ISagaStateData> HandleVerifyingUserEvent(Event<ISagaStateData> fsmEvent)
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
                        new ErrorStateData(data.TransactionId, "User does not have permission to post to group"));

                case StateTimeout _:
                    return GoTo(SagaState.Error).Using(
                        new ErrorStateData(fsmEvent.StateData.TransactionId, "Request timed out while verifying user."));

                default:
                    GoTo(SagaState.Error).Using(
                        new ErrorStateData(fsmEvent.StateData.TransactionId, $"Received unknown event state during VerifyingUser state: {fsmEvent.FsmEvent}"));
                    break;
            }

            return null;
        }

        public FSMBase.State<SagaState, ISagaStateData> HandleIdleEvent(Event<ISagaStateData> ev)
        {
            if (ev.FsmEvent is CreateUserPostRequestCommand command && ev.StateData is Uninitialized)
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
    }
}
