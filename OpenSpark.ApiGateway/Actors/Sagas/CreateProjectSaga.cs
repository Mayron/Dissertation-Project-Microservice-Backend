using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Commands.SagaExecutionCommands;
using System;
using System.Collections.Generic;
using OpenSpark.ApiGateway.StateData;
using OpenSpark.Domain;
using OpenSpark.Shared.Events.CreatePost;
using OpenSpark.Shared.Events.CreateProject;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
    public class CreateProjectSaga : FSM<CreateProjectSaga.SagaState, ISagaStateData>
    {
        private readonly IActorSystemService _actorSystemService;
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
            public User User { get; set; }
        }

        public CreateProjectSaga(IActorSystemService actorSystemService, IFirestoreService firestoreService)
        {
            _actorSystemService = actorSystemService;
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
                // Send command to Groups context to create new group
                _actorSystemService.SendRemoteSagaMessage(RemoteSystem.Projects, Self,
                    new CreateProjectCommand
                    {
                        User = command.User,
                        Name = command.Name,
                        About = command.About,
                        Tags = command.Tags,
                        Visibility = command.Visibility
                    });

                // go to next state
                return GoTo(SagaState.CreatingProject).Using(new ProcessingStateData
                {
                    TransactionId = command.TransactionId,
                    User = command.User,
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

            _actorSystemService.SendRemoteSagaMessage(RemoteSystem.Projects, Self,
                new DeleteProjectCommand
                {
                    ProjectId = @event.Project.Id
                });

            _actorSystemService.SendSagaFailedMessage(StateData.TransactionId,
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
            _actorSystemService.SendSagaSucceededMessage(StateData.TransactionId,
                "Your project is ready to use!",
                new Dictionary<string, string>
                {
                    ["projectId"] = ravenId.ConvertToEntityId()
                });

            return Stop();
        }
    }
}