using Akka.Actor;
using Akka.Configuration;
using System;
using System.IO;
using OpenSpark.Teams.Actors;

namespace OpenSpark.Teams
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configString = File.ReadAllText("teams-system.conf");
            var config = ConfigurationFactory.ParseString(configString);

            // Create actor system
            using var actorSystem = ActorSystem.Create("TeamsSystem", config);

            // Create actors for system
            actorSystem.ActorOf(Props.Create<TeamManagerActor>(), "TeamManager");

            Console.WriteLine("Teams actor system created.");
            Console.ReadKey();
        }
    }
}