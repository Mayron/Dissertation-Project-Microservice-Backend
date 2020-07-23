using System;
using System.IO;
using Akka.Actor;
using Akka.Configuration;
using OpenSpark.Discussions.Actors;

namespace OpenSpark.Discussions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configString = File.ReadAllText("discussions-system.conf");
            var config = ConfigurationFactory.ParseString(configString);

            // Create actor system
            using var actorSystem = ActorSystem.Create("DiscussionsSystem", config);

            // Create actors for system
            actorSystem.ActorOf(Props.Create<DiscussionManagerActor>(), "DiscussionManager");

            Console.WriteLine("Discussions actor system created.");
            Console.ReadKey();
        }
    }
}
