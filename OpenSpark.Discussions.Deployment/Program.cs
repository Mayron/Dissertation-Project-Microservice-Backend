using System;
using System.IO;
using Akka.Actor;
using Akka.Configuration;
using OpenSpark.Discussions.Actors;

namespace OpenSpark.Discussions.Deployment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configString = File.ReadAllText("actor-system.conf");
            var config = ConfigurationFactory.ParseString(configString);

            using var actorSystem = ActorSystem.Create("DiscussionsSystem", config);
            Console.WriteLine("Discussions actor system created.");

            actorSystem.ActorOf(Props.Create<UserManagerActor>(), "UserManager");

            Console.ReadKey();
        }
    }
}
