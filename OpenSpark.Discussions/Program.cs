using Akka.Actor;
using System;

namespace OpenSpark.Discussions
{
    class Program
    {

        static void Main(string[] args)
        {
            using (var actorSystem = ActorSystem.Create("DiscussionsSystem"))
            {
                Console.WriteLine("Discussions actor system created.");

                //            Props userManagerActorProps = Props.Create<UserManagerActor>();
                //            IActorRef userManagerRef = _actorSystem.ActorOf(userManagerActorProps, "UserManager");

                Console.ReadLine(); // pause execution

                actorSystem.Terminate().Wait();
            }
        }
    }
}
