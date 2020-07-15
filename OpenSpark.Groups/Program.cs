using System;
using System.IO;
using Akka.Actor;
using Akka.Configuration;
using OpenSpark.Groups.Actors;

namespace OpenSpark.Groups
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configString = File.ReadAllText("groups-system.conf");
            var config = ConfigurationFactory.ParseString(configString);

            // Create actor system
            using var actorSystem = ActorSystem.Create("GroupsSystem", config);

            // Create actors for system
            actorSystem.ActorOf(Props.Create<GroupManagerActor>(), "GroupManager");

            Console.WriteLine("Groups actor system created.");
            Console.ReadKey();
        }
    }
}
