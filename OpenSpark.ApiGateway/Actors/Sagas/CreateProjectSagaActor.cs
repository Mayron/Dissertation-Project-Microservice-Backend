using Akka.Actor;
using OpenSpark.ApiGateway.Models.StateData;
using OpenSpark.ApiGateway.Models.StateData.CreateProject;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Commands.SagaExecutionCommands;
using OpenSpark.Shared.Events.Sagas;
using OpenSpark.Shared.Events.Sagas.CreatePost;
using OpenSpark.Shared.Events.Sagas.CreateProject;
using System;
using System.Collections.Generic;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
    public class CreateProjectSagaActor : FSM<CreateProjectSagaActor.SagaState, ISagaStateData>
    {
        private readonly IActorSystemService _actorSystemService;
        private readonly IFirestoreService _firestoreService;

        public enum SagaState
        {
            Idle,
            CreatingProject,
            RollingBack
        }

        public CreateProjectSagaActor(IActorSystemService actorSystemService, IFirestoreService firestoreService)
        {
            _actorSystemService = actorSystemService;
            _firestoreService = firestoreService;

            StartWith(SagaState.Idle, IdleStateData.Instance);

            When(SagaState.Idle, HandleIdleEvents);
            When(SagaState.CreatingProject, HandleCreatingProjectEvents);
            When(SagaState.RollingBack, HandleRollingBackEvents);
        }

        private State<SagaState, ISagaStateData> HandleIdleEvents(Event<ISagaStateData> fsmEvent)
        {
            if (fsmEvent.FsmEvent is ExecuteCreateProjectSagaCommand command)
            {
                // Send command to Groups context to create new group
                _actorSystemService.SendProjectsMessage(new CreateProjectCommand
                {
                    TransactionId = command.TransactionId,
                    User = command.User,
                    Name = command.Name,
                    About = command.About,
                    Tags = command.Tags,
                }, Self);

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

            Console.WriteLine($"Rolling back {nameof(CreatePostSagaActor)}.");

            _actorSystemService.SendProjectsMessage(new DeleteProjectCommand
            {
                TransactionId = @event.TransactionId,
                ProjectId = @event.Project.Id
            }, Self);

            _actorSystemService.CallbackHandler.Tell(new SagaMessageEmittedEvent
            {
                TransactionId = StateData.TransactionId,
                Message = "Oops! Something went wrong while trying to create your project.",
                Success = false
            });

            return GoTo(SagaState.RollingBack);
        }

        private State<SagaState, ISagaStateData> HandleRollingBackEvents(Event<ISagaStateData> fsmEvent)
        {
            if (!(fsmEvent.FsmEvent is ProjectDeletedEvent)) return null;

            Console.WriteLine($"Successfully rolled back {nameof(CreatePostSagaActor)}.");
            return Stop();
        }

        private State<SagaState, ISagaStateData> FinishSuccessfully(string ravenId)
        {
            _actorSystemService.CallbackHandler.Tell(new SagaMessageEmittedEvent
            {
                TransactionId = StateData.TransactionId,
                Message = "Your project is ready to use!",
                Success = true,
                Args = new Dictionary<string, string>
                {
                    ["projectId"] = ravenId.ConvertToEntityId()
                }
            });

            return Stop();
        }
    }
}