using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.ApiGateway.StateData;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Events.CreatePost;
using OpenSpark.Shared.Events.CreateProject;
using System;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
    public class CreateProjectSaga : FSM<CreateProjectSaga.SagaState, ISagaStateData>
    {
        private readonly IActorSystem _actorSystem;
        private readonly IFirestoreService _firestoreService;

        public enum SagaState
        {
            Idle,
            CreatingProject,
            RollingBack
        }

        private class ProcessingStateData : ISagaStateData
        {
            public Guid TransactionId { get; set; }
            public MetaData MetaData { get; set; }
            public User User { get; set; }
        }

        public CreateProjectSaga(IActorSystem actorSystem, IFirestoreService firestoreService)
        {
            _actorSystem = actorSystem;
            _firestoreService = firestoreService;

            StartWith(SagaState.Idle, IdleSagaStateData.Instance);

            When(SagaState.Idle, HandleIdleEvents);
            When(SagaState.CreatingProject, HandleCreatingProjectEvents);
            When(SagaState.RollingBack, HandleRollingBackEvents);
        }

        private State<SagaState, ISagaStateData> HandleIdleEvents(Event<ISagaStateData> fsmEvent)
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
                return GoTo(SagaState.CreatingProject).Using(new ProcessingStateData
                {
                    TransactionId = transactionId,
                    User = command.User,
                    MetaData = command.MetaData
                });
            }

            return null;
        }

        private State<SagaState, ISagaStateData> HandleCreatingProjectEvents(Event<ISagaStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is ProjectCreatedEvent @event) || !(StateData is ProcessingStateData data))
                return null;

            // Update user on firestore
            var success = _firestoreService.AddUserToProjectsAsync(data.User, @event.Project).Result;

            if (success) return FinishSuccessfully(@event.Project.Id);

            Console.WriteLine($"Rolling back {nameof(CreatePostSaga)}.");

            var nextCommand = new DeleteProjectCommand
            {
                ProjectId = @event.Project.Id
            };

            _actorSystem.SendSagaMessage(nextCommand, RemoteSystem.Projects, StateData.TransactionId, Self);

            _actorSystem.SendErrorToClient(StateData.MetaData,
                "Oops! Something went wrong while trying to create your project.");

            return GoTo(SagaState.RollingBack);
        }

        private State<SagaState, ISagaStateData> HandleRollingBackEvents(Event<ISagaStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is ProjectDeletedEvent)) return null;

            Console.WriteLine($"Successfully rolled back {nameof(CreatePostSaga)}.");
            return Stop();
        }

        private State<SagaState, ISagaStateData> FinishSuccessfully(string ravenId)
        {
            _actorSystem.SendPayloadToClient(StateData.MetaData, ravenId.ConvertToEntityId());
            return Stop();
        }
    }
}