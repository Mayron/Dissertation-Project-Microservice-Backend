using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.ApiGateway.StateData;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Events;
using OpenSpark.Shared.Events.ConnectProject;
using OpenSpark.Shared.Events.CreateGroup;
using OpenSpark.Shared.ViewModels;
using System;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
    public class CreateGroupSaga : FSM<CreateGroupSaga.SagaState, ISagaStateData>
    {
        private readonly IActorSystem _actorSystem;
        private readonly IFirestoreService _firestoreService;

        public enum SagaState
        {
            Idle,
            CreatingGroup,
            UpdateConnectedProjects,
            RollingBack
        }

        private class ProcessingStateData : ISagaStateData
        {
            public Guid TransactionId { get; set; }
            public MetaData MetaData { get; set; }
            public int SuccessfulConnections { get; set; }
            public int FailedConnections { get; set; }
            public string GroupId { get; set; }
            public ExecuteCreateGroupSagaCommand Command { get; set; }
        }

        public CreateGroupSaga(IActorSystem actorSystem, IFirestoreService firestoreService)
        {
            _actorSystem = actorSystem;
            _firestoreService = firestoreService;

            StartWith(SagaState.Idle, IdleSagaStateData.Instance);

            When(SagaState.Idle, HandleIdleEvents);
            When(SagaState.CreatingGroup, HandleCreatingGroupEvents);
            When(SagaState.UpdateConnectedProjects, HandleUpdateConnectedProjectsEvents);
            When(SagaState.RollingBack, HandleRollingBackEvents);

            WhenUnhandled(ev =>
            {
                switch (ev.FsmEvent)
                {
                    case StateTimeout _:
                        return StopAndSendError("Oops! Request timed out while verifying request.");

                    case ErrorEvent error:
                        Console.WriteLine($"CreateGroupSaga received error: {error.Message}");
                        return StopAndSendError(error.Message);

                    default:
                        Console.WriteLine($"CreatePostSaga received unexpected message: {ev.FsmEvent}. Current State: {StateName}");
                        return Stay();
                }
            });
        }

        private State<SagaState, ISagaStateData> HandleIdleEvents(Event<ISagaStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is ExecuteCreateGroupSagaCommand command)) return null;

            var nextCommand = new CreateGroupCommand
            {
                User = command.User,
                Name = command.Name,
                About = command.About,
                CategoryId = command.CategoryId,
                Visibility = command.Visibility,
                Tags = command.Tags,
            };

            var transactionId = command.MetaData.ParentId;

            // Send command to Groups context to create new group
            _actorSystem.SendSagaMessage(nextCommand, RemoteSystem.Groups, transactionId, Self);

            // go to next state
            return GoTo(SagaState.CreatingGroup).Using(new ProcessingStateData
            {
                TransactionId = transactionId,
                Command = command,
                MetaData = command.MetaData
            });
        }

        private State<SagaState, ISagaStateData> HandleCreatingGroupEvents(Event<ISagaStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is GroupCreatedEvent @event) || !(StateData is ProcessingStateData data))
                return null;

            // Update user on firestore
            var success = _firestoreService.AddUserToGroupsAsync(data.Command.User, @event.GroupId).Result;

            if (success)
            {
                if (data.Command.Connecting.Count <= 0) return FinishSuccessfully(@event.GroupId);

                var command = new ConnectAllProjectsCommand
                {
                    GroupId = @event.GroupId,
                    ProjectIds = data.Command.Connecting,
                    GroupVisibility = @event.GroupVisibility
                };

                _actorSystem.SendSagaMessage(command, RemoteSystem.Projects, data.TransactionId, Self);

                data.GroupId = @event.GroupId;
                return GoTo(SagaState.UpdateConnectedProjects);
            }

            Console.WriteLine($"Rolling back {nameof(CreateGroupSaga)}.");

            var deleteCommand = new DeleteGroupCommand { GroupId = @event.GroupId };
            _actorSystem.SendSagaMessage(deleteCommand, RemoteSystem.Groups, data.TransactionId, Self);

            _actorSystem.SendErrorToClient(data.MetaData,
                "Oops! Something went wrong while trying to create your group.");

            return GoTo(SagaState.RollingBack);
        }

        private State<SagaState, ISagaStateData> HandleUpdateConnectedProjectsEvents(Event<ISagaStateData> fsmEvent)
        {
            switch (fsmEvent.FsmEvent)
            {
                case ProjectConnectedEvent @event when StateData is ProcessingStateData data:
                    data.Command.Connecting.Remove(@event.ProjectId);
                    data.SuccessfulConnections += 1;

                    if (data.Command.Connecting.Count == 0) return FinishSuccessfully(data.GroupId);
                    break;

                case ProjectFailedToConnectEvent @event when StateData is ProcessingStateData data:
                    data.Command.Connecting.Remove(@event.ProjectId);
                    data.FailedConnections += 1;

                    if (data.Command.Connecting.Count == 0) return FinishSuccessfully(data.GroupId);
                    break;
            }

            return null;
        }

        private State<SagaState, ISagaStateData> HandleRollingBackEvents(Event<ISagaStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is GroupDeletedEvent)) return null;

            Console.WriteLine($"Successfully rolled back {nameof(CreateGroupSaga)}.");
            return Stop();
        }

        private State<SagaState, ISagaStateData> FinishSuccessfully(string groupId)
        {
            if (!(StateData is ProcessingStateData data)) return null;

            var viewModel = new GroupCreatedViewModel
            {
                SuccessfulConnections = data.SuccessfulConnections,
                FailedConnections = data.FailedConnections,
                GroupId = groupId.ConvertToClientId()
            };

            _actorSystem.SendPayloadToClient(data.MetaData, viewModel);
            return Stop();
        }

        private State<SagaState, ISagaStateData> StopAndSendError(string errorMessage)
        {
            _actorSystem.SendErrorToClient(StateData.MetaData, errorMessage);
            return Stop();
        }
    }
}