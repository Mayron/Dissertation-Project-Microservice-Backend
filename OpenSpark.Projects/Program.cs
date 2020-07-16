using Akka.Actor;
using Akka.Configuration;
using OpenSpark.Projects.Actors;
using System;
using System.IO;

namespace OpenSpark.Projects
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configString = File.ReadAllText("projects-system.conf");
            var config = ConfigurationFactory.ParseString(configString);

            // Create actor system
            using var actorSystem = ActorSystem.Create("ProjectsSystem", config);

            // Create actors for system
            actorSystem.ActorOf(Props.Create<ProjectManagerActor>(), "ProjectManager");

            Console.WriteLine("Projects actor system created.");
            Console.ReadKey();
        }
    }
}