using System;
using Akka.Actor;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Queries;

namespace OpenSpark.Teams.Actors
{
    public class TeamManagerActor : ReceiveActor
    {
        public TeamManagerActor()
        {
            var queryPool = Context.ActorOf(TeamQueryActor.Props, "TeamQueryPool");
            var commandPool = Context.ActorOf(TeamCommandActor.Props, "TeamCommandPool");

            // Pools
            Receive<IQuery>(command => queryPool.Forward(command));
//            Receive<ICommand>(command => commandPool.Forward(command));

            Receive<ICommand>(command =>
            {
                Console.WriteLine("triggered");
                commandPool.Forward(command);
            });
        }
    }
}