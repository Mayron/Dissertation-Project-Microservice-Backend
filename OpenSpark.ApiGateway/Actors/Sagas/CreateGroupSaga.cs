using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.ApiGateway.StateData;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Events;
using OpenSpark.Shared.Events.ConnectProject;
using OpenSpark.Shared.Events.CreateGroup;
using System;
using System.Collections.Generic;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
    public class CreateGroupSaga : FSM<CreateGroupSaga.SagaState, ISagaStateData>
    {
        private readonly IActorSystemService _actorSystemService;
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
            public User User { get; set; }
            public List<string> Connecting { get; set; }
            public int SuccessfulConnections { get; set; }
            public int FailedConnections { get; set; }
            public string GroupId { get; set; }
        }

        public CreateGroupSaga(IActorSystemService actorSystemService, IFirestoreService firestoreService)
        {
            _actorSystemService = actorSystemService;
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
            if (fsmEvent.FsmEvent is ExecuteCreateGroupSagaCommand command)
            {
                // Send command to Groups context to create new group
                _actorSystemService.SendRemoteSagaMessage(RemoteSystem.Groups, Self,
                    new CreateGroupCommand
                    {
                        User = command.User,
                        Name = command.Name,
                        About = command.About,
                        CategoryId = command.CategoryId,
                        Visibility = command.Visibility,
                        Tags = command.Tags,
                    });

                // go to next state
                return GoTo(SagaState.CreatingGroup).Using(new ProcessingStateData
                {
                    TransactionId = command.TransactionId,
                    User = command.User,
                    Connecting = command.Connecting
                });
            }

            return null;
        }

        private State<SagaState, ISagaStateData> HandleCreatingGroupEvents(Event<ISagaStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is GroupCreatedEvent @event) || !(StateData is ProcessingStateData data))
                return null;

            // Update user on firestore
            var success = _firestoreService.AddUserToGroupsAsync(data.User, @event.Group).Result;

            if (success)
            {
                if (data.Connecting.Count <= 0) return FinishSuccessfully(@event.Group.Id);

                _actorSystemService.SendRemoteSagaMessage(RemoteSystem.Projects, Self,
                    new ConnectAllProjectsCommand
                    {
                        GroupId = @event.Group.Id,
                        ProjectIds = data.Connecting,
                        GroupVisibility = @event.Group.Visibility
                    });

                return GoTo(SagaState.UpdateConnectedProjects).Using(new ProcessingStateData
                {
                    TransactionId = StateData.TransactionId,
                    Connecting = data.Connecting,
                    GroupId = @event.Group.Id
                });
            }

            Console.WriteLine($"Rolling back {nameof(CreateGroupSaga)}.");

            _actorSystemService.SendRemoteSagaMessage(RemoteSystem.Groups, Self,
                new DeleteGroupCommand
                {
                    GroupId = @event.Group.Id
                });

            _actorSystemService.SendSagaFailedMessage(StateData.TransactionId,
                "Oops! Something went wrong while trying to create your group.");

            return GoTo(SagaState.RollingBack);
        }

        private State<SagaState, ISagaStateData> HandleUpdateConnectedProjectsEvents(Event<ISagaStateData> fsmEvent)
        {
            switch (fsmEvent.FsmEvent)
            {
                case ProjectConnectedEvent @event when StateData is ProcessingStateData data:
                    data.Connecting.Remove(@event.ProjectId);
                    data.SuccessfulConnections += 1;
                    if (data.Connecting.Count == 0) return FinishSuccessfully(data.GroupId);
                    break;

                case ProjectFailedToConnectEvent @event when StateData is ProcessingStateData data:
                    data.Connecting.Remove(@event.ProjectId);
                    data.FailedConnections += 1;
                    if (data.Connecting.Count == 0) return FinishSuccessfully(data.GroupId);
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
            if (StateData is ProcessingStateData data)
            {
                if (data.SuccessfulConnections > 0)
                {
                    _actorSystemService.SendSagaSucceededMessage(StateData.TransactionId,
                        $"{data.SuccessfulConnections} projects connected to your group.");
                }

                if (data.FailedConnections > 0)
                {
                    _actorSystemService.SendSagaFailedMessage(StateData.TransactionId,
                        $"{data.FailedConnections} projects could not be connected to your group, possibly due to their visibility status.");
                }
            }

            _actorSystemService.SendSagaSucceededMessage(StateData.TransactionId,
                "Your group is ready to use!",
                new Dictionary<string, string>
                {
                    ["groupId"] = groupId.ConvertToEntityId()
                }
            );

            return Stop();
        }

        private State<SagaState, ISagaStateData> StopAndSendError(string errorMessage)
        {
            _actorSystemService.SendSagaFailedMessage(StateData.TransactionId, errorMessage);
            return Stop();
        }
    }
}