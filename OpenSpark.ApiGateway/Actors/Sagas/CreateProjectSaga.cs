using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.ApiGateway.StateData;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Commands.Teams;
using OpenSpark.Shared.Domain;
using OpenSpark.Shared.Events.CreatePost;
using OpenSpark.Shared.Events.CreateProject;
using OpenSpark.Shared.Events.Payloads;
using System;
using System.Threading;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
    public class CreateProjectSaga : FSM<CreateProjectSaga.SagaState, CreateProjectSaga.CreateProjectStateData>
    {
        private readonly IActorSystem _actorSystem;
        private readonly IFirestoreService _firestoreService;

        public enum SagaState
        {
            Idle,
            CreatingProject,
            RollingBack,
            CreatingTeams
        }

        public class CreateProjectStateData : ISagaStateData
        {
            public Guid TransactionId { get; set; }
            public MetaData MetaData { get; set; }
            public User User { get; set; }
            public string ProjectId { get; set; }
        }

        public CreateProjectSaga(IActorSystem actorSystem, IFirestoreService firestoreService)
        {
            _actorSystem = actorSystem;
            _firestoreService = firestoreService;

            StartWith(SagaState.Idle, new CreateProjectStateData());

            When(SagaState.Idle, HandleIdleEvents);
            When(SagaState.CreatingProject, HandleCreatingProjectEvents);
            When(SagaState.CreatingTeams, HandleCreatingTeamsEvents);
            When(SagaState.RollingBack, HandleRollingBackEvents);
        }

        private State<SagaState, CreateProjectStateData> HandleIdleEvents(Event<CreateProjectStateData> fsmEvent)
        {
            if (fsmEvent.FsmEvent is ExecuteCreateProjectSagaCommand command)
            {
                var nextCommand = new CreateProjectCommand
                {
                    User = command.User,
                    Name = command.Name,
                    About = command.About,
                    Tags = command.Tags,
                    Visibility = command.Visibility
                };

                var transactionId = command.MetaData.ParentId;

                // Send command to Groups context to create new group
                _actorSystem.SendSagaMessage(nextCommand, RemoteSystem.Projects, transactionId, Self);

                // go to next state
                StateData.TransactionId = transactionId;
                StateData.User = command.User;
                StateData.MetaData = command.MetaData;

                return GoTo(SagaState.CreatingProject);
            }

            return null;
        }

        private State<SagaState, CreateProjectStateData> HandleCreatingProjectEvents(Event<CreateProjectStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is ProjectCreatedEvent @event))
                return null;

            StateData.ProjectId = @event.ProjectId;

            // Update user on firestore
            var success = _firestoreService.AddUserToProjectsAsync(StateData.User, CancellationToken.None, @event.ProjectId).Result;

            if (success)
            {
                var createDefaultTeamsCommand = new CreateDefaultTeamsCommand
                {
                    User = StateData.User,
                    ProjectId = StateData.ProjectId
                };

                _actorSystem.SendSagaMessage(createDefaultTeamsCommand, RemoteSystem.Teams, StateData.TransactionId, Self);
                return GoTo(SagaState.CreatingTeams);
            }

            return Rollback();
        }

        private State<SagaState, CreateProjectStateData> HandleCreatingTeamsEvents(Event<CreateProjectStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is PayloadEvent @event)) return null;

            if (@event.Errors == null) return FinishSuccessfully(StateData.ProjectId);

            foreach (var error in @event.Errors)
                Console.WriteLine($"CreateProjectSaga error: {error}");

            return Rollback();
        }

        private State<SagaState, CreateProjectStateData> HandleRollingBackEvents(Event<CreateProjectStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is ProjectDeletedEvent)) return null;

            Console.WriteLine($"Successfully rolled back {nameof(CreatePostSaga)}.");
            return Stop();
        }

        private State<SagaState, CreateProjectStateData> Rollback()
        {
            Console.WriteLine($"Rolling back {nameof(CreatePostSaga)}.");

            var deleteProjectCommand = new DeleteProjectCommand
            {
                User = StateData.User,
                ProjectId = StateData.ProjectId
            };

            _actorSystem.SendSagaMessage(deleteProjectCommand, RemoteSystem.Projects, StateData.TransactionId, Self);

            _actorSystem.SendErrorToClient(StateData.MetaData,
                "Oops! Something went wrong while trying to create your project.");

            return GoTo(SagaState.RollingBack);
        }

        private State<SagaState, CreateProjectStateData> FinishSuccessfully(string projectId)
        {
            _actorSystem.SendPayloadToClient(StateData.MetaData, projectId);
            return Stop();
        }
    }
}