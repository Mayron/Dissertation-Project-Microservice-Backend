using Akka.Actor;
using OpenSpark.ApiGateway.Models.StateData;
using OpenSpark.ApiGateway.Models.StateData.CreateGroup;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Events.Sagas;
using OpenSpark.Shared.Events.Sagas.CreateGroup;
using System;
using System.Collections.Generic;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Commands.SagaExecutionCommands;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
    public class CreateGroupSagaActor : FSM<CreateGroupSagaActor.SagaState, ISagaStateData>
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

        public CreateGroupSagaActor(IActorSystemService actorSystemService, IFirestoreService firestoreService)
        {
            _actorSystemService = actorSystemService;
            _firestoreService = firestoreService;

            StartWith(SagaState.Idle, IdleStateData.Instance);

            When(SagaState.Idle, HandleIdleEvents);
            When(SagaState.CreatingGroup, HandleCreatingGroupEvents);
            When(SagaState.UpdateConnectedProjects, HandleUpdateConnectedProjectsEvents);
            When(SagaState.RollingBack, HandleRollingBackEvents);
        }

        private State<SagaState, ISagaStateData> HandleIdleEvents(Event<ISagaStateData> fsmEvent)
        {
            if (fsmEvent.FsmEvent is ExecuteCreateGroupSagaCommand command)
            {
                // Send command to Groups context to create new group
                _actorSystemService.SendGroupsMessage(new CreateGroupCommand
                {
                    TransactionId = command.TransactionId,
                    User = command.User,
                    Name = command.Name,
                    About = command.About,
                    CategoryId = command.CategoryId,
                    Tags = command.Tags,
                    Connected = command.Connecting
                }, Self);

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

                _actorSystemService.SendProjectsMessage(new ConnectAllProjectsCommand
                {
                    TransactionId = @event.TransactionId,
                    GroupId = @event.Group.Id,
                    ProjectIds = data.Connecting
                }, Self);

                return GoTo(SagaState.UpdateConnectedProjects).Using(new ProcessingStateData
                {
                    TransactionId = StateData.TransactionId,
                    Connecting = data.Connecting,
                    GroupId = @event.Group.Id
                });
            }

            Console.WriteLine($"Rolling back {nameof(CreateGroupSagaActor)}.");

            _actorSystemService.SendGroupsMessage(new DeleteGroupCommand
            {
                TransactionId = @event.TransactionId,
                GroupId = @event.Group.Id
            }, Self);

            _actorSystemService.CallbackHandler.Tell(new SagaMessageEmittedEvent
            {
                TransactionId = StateData.TransactionId,
                Message = "Oops! Something went wrong while trying to create your group.",
                Success = false
            });

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

            Console.WriteLine($"Successfully rolled back {nameof(CreateGroupSagaActor)}.");
            return Stop();
        }

        private State<SagaState, ISagaStateData> FinishSuccessfully(string groupId)
        {
            if (StateData is ProcessingStateData data)
            {
                if (data.SuccessfulConnections > 0)
                {
                    _actorSystemService.CallbackHandler.Tell(new SagaMessageEmittedEvent
                    {
                        TransactionId = StateData.TransactionId,
                        Message = $"{data.SuccessfulConnections} projects connected to your group.",
                        Success = true
                    });
                }

                if (data.FailedConnections > 0)
                {
                    _actorSystemService.CallbackHandler.Tell(new SagaMessageEmittedEvent
                    {
                        TransactionId = StateData.TransactionId,
                        Message = $"{data.FailedConnections} projects could not be connected to your group, possibly due to their visibility status.",
                        Success = false
                    });
                }
            }

            _actorSystemService.CallbackHandler.Tell(new SagaMessageEmittedEvent
            {
                TransactionId = StateData.TransactionId,
                Message = "Your group is ready to use!",
                Success = true,
                Args = new Dictionary<string, string>
                {
                    ["groupId"] = groupId.ConvertToEntityId()
                }
            });

            return Stop();
        }
    }
}