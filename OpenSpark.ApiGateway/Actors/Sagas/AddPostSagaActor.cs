using System;
using Akka.Actor;
using OpenSpark.ApiGateway.Models;
using OpenSpark.ApiGateway.Models.StateData;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Events;
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
        }

        public AddPostSagaActor(IActorSystemService actorSystemService, IEventEmitterService eventEmitter)
        {
            _actorSystemService = actorSystemService;
            _eventEmitter = eventEmitter;

            StartWith(SagaState.Idle, Uninitialized.Instance);

            When(SagaState.Idle, HandleIdleEvents);
            When(SagaState.VerifyingUser, HandleVerifyingUserEvents); // TimeSpan.FromSeconds(5)
            When(SagaState.AddingPost, HandleAddingPostEvents); // TimeSpan.FromSeconds(5)

            WhenUnhandled(ev =>
            {
                Console.WriteLine($"Unexpected message received: {ev.FsmEvent}. Current State: {StateName}");

                if (!(ev.FsmEvent is ErrorEvent error)) return Stay();

                Console.WriteLine(error.Message);
                return StopAndSendError("Oops! Something unexpected happened.");
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
                }, Self);

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

                    // User verified successfully. Begin adding post.
                    _actorSystemService.SendDiscussionsCommand(new AddPostCommand
                    {
                        TransactionId = data.TransactionId,
                    }, Self);

                    return GoTo(SagaState.AddingPost).Using(new AddingPostStateData(data.TransactionId));

                case UserVerificationFailedEvent @event when fsmEvent.StateData is VerifyingUserStateData data:
                    if (@event.TransactionId != data.TransactionId)
                    {
                        Console.WriteLine($"Unexpected transaction Id for UserVerificationFailedEvent: {@event.TransactionId}");
                        return Stay();
                    }

                    return StopAndSendError("You do not have permission to post to this group.");

                case StateTimeout _:
                    return StopAndSendError("Oops! Request timed out while verifying request.");
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
                    
                    _eventEmitter.BroadcastToGroup(@event.GroupId, "PostAdded", @event.Post);

                    _actorSystemService.CallbackHandler.Tell(new SagaFinishedEvent
                    {
                        TransactionId = StateData.TransactionId,
                        Message = "New post created successfully.",
                        Success = true
                    });

                    return Stop();

                case StateTimeout _:
                    return StopAndSendError("Oops! Request timed out while created post.");
            }

            return null;
        }

        private State<SagaState, ISagaStateData> StopAndSendError(string errorMessage)
        {
            _actorSystemService.CallbackHandler.Tell(new SagaFinishedEvent
            {
                TransactionId = StateData.TransactionId,
                Message = errorMessage,
                Success = false
            });

            return Stop();
        }
    }
}
