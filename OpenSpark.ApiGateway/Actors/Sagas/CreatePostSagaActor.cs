using Akka.Actor;
using OpenSpark.ApiGateway.Models.StateData;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Events.Sagas;
using OpenSpark.Shared.Events.Sagas.CreatePost;
using System;
using OpenSpark.Shared.Commands.Posts;
using OpenSpark.Shared.Commands.SagaExecutionCommands;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
    public class CreatePostSagaActor : FSM<CreatePostSagaActor.SagaState, ISagaStateData>
    {
        private readonly IActorSystemService _actorSystemService;
        private readonly IEventEmitterService _eventEmitter;

        public enum SagaState
        {
            Idle,
            VerifyingUser,
            CreatingPost,
        }

        public CreatePostSagaActor(IActorSystemService actorSystemService, IEventEmitterService eventEmitter)
        {
            _actorSystemService = actorSystemService;
            _eventEmitter = eventEmitter;

            StartWith(SagaState.Idle, IdleStateData.Instance);

            When(SagaState.Idle, HandleIdleEvents);
            When(SagaState.VerifyingUser, HandleVerifyingUserEvents); // TimeSpan.FromSeconds(5)
            When(SagaState.CreatingPost, HandleAddingPostEvents); // TimeSpan.FromSeconds(5)

            WhenUnhandled(ev =>
            {
                if (!(ev.FsmEvent is SagaErrorEvent error))
                {
                    Console.WriteLine($"Unexpected message received: {ev.FsmEvent}. Current State: {StateName}");
                    return Stay();
                };

                if (error.TransactionId != StateData.TransactionId)
                {
                    Console.WriteLine($"Expected SagaErrorEvent to have transaction Id {StateData.TransactionId} but received {error.TransactionId}. Error message: {error.Message}");
                    return Stay();
                }

                Console.WriteLine(error.Message);
                return StopAndSendError("Oops! Something unexpected happened.");
            });
        }

        public State<SagaState, ISagaStateData> HandleIdleEvents(Event<ISagaStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is ExecuteAddPostSagaCommand command)) return null;

            _actorSystemService.SendGroupsMessage(new VerifyUserPostRequestCommand
            {
                TransactionId = command.TransactionId,
                UserId = command.User.AuthUserId,
                GroupId = command.GroupId,
            }, Self);

            return GoTo(SagaState.VerifyingUser).Using(new SagaStateData(command.TransactionId));
        }

        private State<SagaState, ISagaStateData> HandleVerifyingUserEvents(Event<ISagaStateData> fsmEvent)
        {
            switch (fsmEvent.FsmEvent)
            {
                case UserVerifiedEvent @event:
                    if (@event.TransactionId != StateData.TransactionId)
                    {
                        Console.WriteLine($"Unexpected transaction Id for UserVerifiedEvent: {@event.TransactionId}");
                        return Stay();
                    }

                    // User verified successfully. Begin adding post.
                    _actorSystemService.SendDiscussionsMessage(new CreatePostCommand
                    {
                        TransactionId = StateData.TransactionId,
                    }, Self);

                    return GoTo(SagaState.CreatingPost);

                case UserVerificationFailedEvent @event:
                    if (@event.TransactionId == StateData.TransactionId)
                        return StopAndSendError("You do not have permission to post to this group.");

                    Console.WriteLine($"Unexpected transaction Id for UserVerificationFailedEvent: {@event.TransactionId}");
                    return Stay();

                case StateTimeout _:
                    return StopAndSendError("Oops! Request timed out while verifying request.");
            }

            return null;
        }

        private State<SagaState, ISagaStateData> HandleAddingPostEvents(Event<ISagaStateData> fsmEvent)
        {
            switch (fsmEvent.FsmEvent)
            {
                case PostAddedEvent @event:
                    if (@event.TransactionId != StateData.TransactionId)
                    {
                        Console.WriteLine($"Unexpected transaction Id for PostAddedEvent: {@event.TransactionId}");
                        return Stay();
                    }

                    _eventEmitter.BroadcastToGroup(@event.GroupId, "PostAdded", @event.Post);

                    _actorSystemService.CallbackHandler.Tell(new SagaMessageEmittedEvent
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
            _actorSystemService.CallbackHandler.Tell(new SagaMessageEmittedEvent
            {
                TransactionId = StateData.TransactionId,
                Message = errorMessage,
                Success = false
            });

            return Stop();
        }
    }
}