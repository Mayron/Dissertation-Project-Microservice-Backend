using Akka.Actor;
using OpenSpark.Shared.Commands.Teams;
using OpenSpark.Shared.Queries;

namespace OpenSpark.Teams.Actors
{
    public class TeamManagerActor : ReceiveActor
    {
        public TeamManagerActor()
        {
            var teamQueryPool = Context.ActorOf(TeamQueryActor.Props, "TeamQueryPool");
            var createTeamPool = Context.ActorOf(CreateTeamActor.Props, "CreateTeamPool");

            // Pools
            Receive<TeamsQuery>(command => teamQueryPool.Forward(command));
            Receive<CreateTeamCommand>(command => createTeamPool.Forward(command));
            Receive<CreateDefaultTeamsCommand>(command => createTeamPool.Forward(command));
        }
    }
}