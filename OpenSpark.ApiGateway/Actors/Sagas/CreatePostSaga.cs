using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Posts;
using OpenSpark.Shared.Commands.SagaExecutionCommands;
using OpenSpark.Shared.Events;
using OpenSpark.Shared.Events.CreatePost;
using System;
using System.Collections.Generic;
using OpenSpark.ApiGateway.StateData;
using OpenSpark.Shared.Commands.Sagas;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
    public class CreatePostSaga : FSM<CreatePostSaga.SagaState, ISagaStateData>
    {
        private readonly IActorSystemService _actorSystemService;

        public enum SagaState
        {
            Idle,
            VerifyingUser,
            CreatingPost,
        }

        private class ProcessingStateData : ISagaStateData
        {
            public Guid TransactionId { get; set; }
            public User User { get; set; }
            public string GroupId { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
            public string GroupName { get; set; }
        }

        public CreatePostSaga(IActorSystemService actorSystemService)
        {
            _actorSystemService = actorSystemService;

            StartWith(SagaState.Idle, IdleSagaStateData.Instance);

            When(SagaState.Idle, HandleIdleEvents);
            When(SagaState.VerifyingUser, HandleVerifyingUserEvents); // TimeSpan.FromSeconds(5)
            When(SagaState.CreatingPost, HandleCreatingPostEvents); // TimeSpan.FromSeconds(5)

            WhenUnhandled(ev =>
            {
                switch (ev.FsmEvent)
                {
                    case StateTimeout _:
                        return StopAndSendError("Oops! Request timed out while verifying request.");

                    case ErrorEvent error:
                        Console.WriteLine($"CreatePostSaga received error: {error.Message}");
                        return StopAndSendError("Oops! Something unexpected happened.");

                    default:
                        Console.WriteLine($"CreatePostSaga received unexpected message: {ev.FsmEvent}. Current State: {StateName}");
                        return Stay();
                }
            });
        }

        public State<SagaState, ISagaStateData> HandleIdleEvents(Event<ISagaStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is ExecuteCreatePostSagaCommand command)) return null;

            _actorSystemService.SendRemoteSagaMessage(RemoteSystem.Groups, Self,
                new VerifyPostRequestCommand
                {
                    User = command.User,
                    GroupId = command.GroupId,
                });

            return GoTo(SagaState.VerifyingUser).Using(new ProcessingStateData
            {
                TransactionId = command.TransactionId,
                User = command.User,
                Title = command.Title,
                Body = command.Body,
                GroupId = command.GroupId
            });
        }

        private State<SagaState, ISagaStateData> HandleVerifyingUserEvents(Event<ISagaStateData> fsmEvent)
        {
            switch (fsmEvent.FsmEvent)
            {
                case UserVerifiedEvent @event when StateData is ProcessingStateData data:
                    // User verified successfully. Begin adding post.
                    _actorSystemService.SendRemoteSagaMessage(RemoteSystem.Discussions, Self,
                        new CreatePostCommand
                        {
                            Title = data.Title,
                            Body = data.Body,
                            User = data.User,
                            GroupId = data.GroupId,
                            GroupVisibility = @event.GroupVisibility
                        });

                    data.GroupName = @event.GroupName;
                    return GoTo(SagaState.CreatingPost);

                case UserVerificationFailedEvent _:
                    return StopAndSendError("You do not have permission to post to this group.");
            }

            return null;
        }

        private State<SagaState, ISagaStateData> HandleCreatingPostEvents(Event<ISagaStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is PostCreatedEvent @event)) return null;

            _actorSystemService.SendSagaSucceededMessage(
                StateData.TransactionId, 
                "New post created successfully.",
                new Dictionary<string, string>
                {
                    ["postId"] = @event.PostId
                });

            return Stop();
        }

        private State<SagaState, ISagaStateData> StopAndSendError(string errorMessage)
        {
            _actorSystemService.SendSagaFailedMessage(StateData.TransactionId, errorMessage);
            return Stop();
        }
    }
}