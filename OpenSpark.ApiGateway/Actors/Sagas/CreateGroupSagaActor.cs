using Akka.Actor;
using OpenSpark.ApiGateway.Models.StateData;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Commands.Sagas.CreateGroup;
using OpenSpark.Shared.Commands.Sagas.ExecutionCommands;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
    public class CreateGroupSagaActor : FSM<CreateGroupSagaActor.SagaState, ISagaStateData>
    {
        private readonly IActorSystemService _actorSystemService;

        public enum SagaState
        {
            Idle,
            CreatingGroup,
            AddingGroupToUser,
            UpdateConnectedProjects
        }

        // TODO: Verify group name is not taken, Generate group ID, add group, Update user on firestore, update connected projects, 

        public CreateGroupSagaActor(IActorSystemService actorSystemService)
        {
            _actorSystemService = actorSystemService;
            StartWith(SagaState.Idle, IdleStateData.Instance);

            When(SagaState.Idle, HandleIdleEvents);
        }

        private State<SagaState, ISagaStateData> HandleIdleEvents(Event<ISagaStateData> fsmEvent)
        {
            if (fsmEvent.FsmEvent is ExecuteCreateGroupSagaCommand command)
            {
                // Send command to context
                _actorSystemService.SendGroupsCommand(new CreateGroupCommand
                {
                    TransactionId = command.TransactionId,
                    Name = command.Group.Name,
                    UserId = command.User.UserId,
                }, Self);

                // go to next state
            }

            return null;
        }
    }
}