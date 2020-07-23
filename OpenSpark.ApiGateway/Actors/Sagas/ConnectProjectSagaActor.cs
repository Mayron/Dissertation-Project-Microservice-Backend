using Akka.Actor;
using OpenSpark.ApiGateway.Models.StateData;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Commands.SagaExecutionCommands;
using OpenSpark.Shared.Events.ConnectProject;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;
using System;
using System.Collections.Generic;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
    public class ConnectProjectSagaActor : FSM<ConnectProjectSagaActor.SagaState, ISagaStateData>
    {
        private readonly IActorSystemService _actorSystemService;
        //        protected ILoggingAdapter Log { get; } = Context.GetLogger();

        public enum SagaState
        {
            Idle,
            ValidatingTargetGroup,
            ConnectingProject,
        }

        private class ProcessingStateData : ISagaStateData
        {
            public Guid TransactionId { get; set; }
            public User User { get; set; }
            public string ProjectId { get; set; }
            public string GroupId { get; set; }
            public string GroupName { get; set; }
        }

        public ConnectProjectSagaActor(IActorSystemService actorSystemService)
        {
            _actorSystemService = actorSystemService;

            StartWith(SagaState.Idle, IdleStateData.Instance);

            When(SagaState.Idle, HandleIdleEvents);
            When(SagaState.ValidatingTargetGroup, HandleValidatingTargetGroupEvent);
            When(SagaState.ConnectingProject, HandleConnectingProjectEvents);
        }

        private State<SagaState, ISagaStateData> HandleIdleEvents(Event<ISagaStateData> fsmEvent)
        {
            if (fsmEvent.FsmEvent is ExecuteConnectProjectSagaCommand command)
            {
                // Send command to Groups context to validate if project is allowed to connect
                _actorSystemService.SendRemoteSagaMessage(RemoteSystem.Groups, Self,
                    new GroupDetailsQuery
                    {
                        User = command.User,
                        GroupId = command.GroupId,
                    });

                // go to next state
                return GoTo(SagaState.ValidatingTargetGroup).Using(new ProcessingStateData
                {
                    TransactionId = command.TransactionId,
                    User = command.User,
                    GroupId = command.GroupId,
                    ProjectId = command.ProjectId
                });
            }

            return null;
        }

        private State<SagaState, ISagaStateData> HandleValidatingTargetGroupEvent(Event<ISagaStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is PayloadEvent @event) || !(StateData is ProcessingStateData data))
                return null;

            if (@event.Errors != null)
            {
                _actorSystemService.SendSagaFailedMessage(StateData.TransactionId, @event.Errors[0]);
                return Stop();
            }

            if (!(@event.Payload is GroupDetailsViewModel groupDetails))
                throw new ActorKilledException($"Unexpected payload: {@event.Payload}");

            _actorSystemService.SendRemoteSagaMessage(RemoteSystem.Projects, Self,
                new ConnectAllProjectsCommand
                {
                    GroupId = data.GroupId,
                    GroupVisibility = groupDetails.Visibility,
                    User = data.User,
                    ProjectIds = new List<string> { data.ProjectId }
                });

            // go to next state
            return GoTo(SagaState.ConnectingProject).Using(new ProcessingStateData
            {
                TransactionId = StateData.TransactionId,
                User = data.User,
                GroupId = data.GroupId,
                ProjectId = data.ProjectId,
                GroupName = groupDetails.Name
            });
        }

        private State<SagaState, ISagaStateData> HandleConnectingProjectEvents(Event<ISagaStateData> fsmEvent)
        {
            switch (fsmEvent.FsmEvent)
            {
                case ProjectConnectedEvent _ when StateData is ProcessingStateData data:
                    _actorSystemService.SendSagaSucceededMessage(StateData.TransactionId,
                        $"Your project has been connected to the {data.GroupName} group.");
                    return Stop();

                case ProjectFailedToConnectEvent @event:
                    _actorSystemService.SendSagaFailedMessage(StateData.TransactionId, @event.Message);
                    return Stop();
            }

            return null;
        }
    }
}