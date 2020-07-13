using System.IO;
using Akka.Actor;
using Akka.Configuration;
using OpenSpark.ApiGateway.Actors;
using OpenSpark.ApiGateway.Services.SDK;

namespace OpenSpark.ApiGateway.Services
{
    public class LocalActorSystemService : ILocalActorSystemService
    {
        public IActorRef CallbackActorRef { get; }
        public ActorSystem LocalSystem { get; }

        public LocalActorSystemService(IEventEmitterService eventEmitter)
        {
            // Create local WebApiSystem
            var configString = File.ReadAllText("webapi-system.conf");
            var config = ConfigurationFactory.ParseString(configString);
            LocalSystem = ActorSystem.Create("WebApiSystem", config);

            // Create local actors for the system
            CallbackActorRef = LocalSystem.ActorOf(Props.Create(() => new CallbackActor(eventEmitter)), "Callback");
        }

        public void Dispose()
        {
            LocalSystem?.Dispose();
        }
    }
}
