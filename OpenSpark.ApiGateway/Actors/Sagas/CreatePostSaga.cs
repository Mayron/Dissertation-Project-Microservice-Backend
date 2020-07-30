using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.ApiGateway.StateData;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Discussions;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Events;
using OpenSpark.Shared.Events.CreatePost;
using System;
using OpenSpark.Shared.Domain;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
    public class CreatePostSaga : FSM<CreatePostSaga.SagaState, ISagaStateData>
    {
        private readonly IActorSystem _actorSystem;

        public enum SagaState
        {
            Idle,
            VerifyingUser,
            CreatingPost,
        }

        private class ProcessingStateData : ISagaStateData
        {
            public Guid TransactionId { get; set; }
            public MetaData MetaData { get; set; }
            public User User { get; set; }
            public string GroupId { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
        }

        public CreatePostSaga(IActorSystem actorSystem)
        {
            _actorSystem = actorSystem;

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

            var nextCommand = new VerifyPostRequestCommand
            {
                User = command.User,
                GroupId = command.GroupId,
            };

            var transactionId = command.MetaData.ParentId;
            _actorSystem.SendSagaMessage(nextCommand, RemoteSystem.Groups, transactionId, Self);

            return GoTo(SagaState.VerifyingUser).Using(new ProcessingStateData
            {
                TransactionId = transactionId,
                MetaData = command.MetaData,
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
                    var nextCommand = new CreatePostCommand
                    {
                        Title = data.Title,
                        Body = data.Body,
                        User = data.User,
                        GroupId = data.GroupId,
                        GroupVisibility = @event.GroupVisibility
                    };

                    _actorSystem.SendSagaMessage(nextCommand, RemoteSystem.Discussions, StateData.TransactionId, Self);

                    return GoTo(SagaState.CreatingPost);

                case UserVerificationFailedEvent _:
                    return StopAndSendError("You do not have permission to post to this group.");
            }

            return null;
        }

        private State<SagaState, ISagaStateData> HandleCreatingPostEvents(Event<ISagaStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is PostCreatedEvent @event)) return null;

            _actorSystem.SendPayloadToClient(StateData.MetaData, @event.PostId);
            return Stop();
        }

        private State<SagaState, ISagaStateData> StopAndSendError(string errorMessage)
        {
            _actorSystem.SendErrorToClient(StateData.MetaData, errorMessage);
            return Stop();
        }
    }
}