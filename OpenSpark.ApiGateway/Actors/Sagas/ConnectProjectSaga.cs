using System;
using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.ApiGateway.StateData;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Events.ConnectProject;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;
using System.Collections.Generic;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
    public class ConnectProjectSaga : FSM<ConnectProjectSaga.SagaState, ISagaStateData>
    {
        private readonly IActorSystemService _actorSystemService;

        public enum SagaState
        {
            Idle,
            ValidatingTargetGroup,
            ConnectingProject,
        }

        private class ProcessingStateData : ISagaStateData
        {
            public string GroupName { get; set; }
            public Guid TransactionId { get; set; }
            public MetaData MetaData { get; set; }
            public ExecuteConnectProjectSagaCommand Command { get; set; }
        }

        public ConnectProjectSaga(IActorSystemService actorSystem)
        {
            _actorSystemService = actorSystem;

            StartWith(SagaState.Idle, IdleSagaStateData.Instance);

            When(SagaState.Idle, HandleIdleEvents);
            When(SagaState.ValidatingTargetGroup, HandleValidatingTargetGroupEvent);
            When(SagaState.ConnectingProject, HandleConnectingProjectEvents);
        }

        private State<SagaState, ISagaStateData> HandleIdleEvents(Event<ISagaStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is ExecuteConnectProjectSagaCommand command)) return null;

            // Send command to Groups context to validate if project is allowed to connect
            var remoteQuery = new GroupDetailsQuery
            {
                User = command.User,
                GroupId = command.GroupId
            };

            var transactionId = command.MetaData.ParentId;
            _actorSystemService.SendSagaMessage(remoteQuery, RemoteSystem.Groups, transactionId, Self);

            // create processing state data
            return GoTo(SagaState.ValidatingTargetGroup).Using(
                new ProcessingStateData
                {
                    Command = command,
                    MetaData = command.MetaData,
                    TransactionId = transactionId
                });
        }

        private State<SagaState, ISagaStateData> HandleValidatingTargetGroupEvent(Event<ISagaStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is PayloadEvent @event) || !(StateData is ProcessingStateData data))
                return null;

            if (@event.Errors != null && @event.Errors.Length > 0)
            {
                _actorSystemService.SendErrorsToClient(data.MetaData, @event.Errors);
                return Stop();
            }

            if (!(@event.Payload is GroupDetailsViewModel groupDetails))
                throw new ActorKilledException($"Unexpected payload: {@event.Payload}");

            var command = new ConnectAllProjectsCommand
            {
                GroupId = data.Command.GroupId,
                GroupVisibility = groupDetails.Visibility,
                User = data.Command.User,
                ProjectIds = new List<string> { data.Command.ProjectId }
            };

            _actorSystemService.SendSagaMessage(command, RemoteSystem.Projects, data.TransactionId, Self);

            data.GroupName = groupDetails.Name;
            return GoTo(SagaState.ConnectingProject);
        }

        private State<SagaState, ISagaStateData> HandleConnectingProjectEvents(Event<ISagaStateData> fsmEvent)
        {
            switch (fsmEvent.FsmEvent)
            {
                case ProjectConnectedEvent _ when StateData is ProcessingStateData data:

                    _actorSystemService.SendPayloadToClient(data.MetaData,
                        $"Your project has been connected to the {data.GroupName} group.");

                    return Stop();

                case ProjectFailedToConnectEvent @event when StateData is ProcessingStateData data:

                    _actorSystemService.SendErrorToClient(data.MetaData, @event.Message);
                    return Stop();
            }

            return null;
        }
    }
}