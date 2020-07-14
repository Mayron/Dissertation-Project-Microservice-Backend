using System.IO;
using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Configuration;
using OpenSpark.ApiGateway.Actors;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Commands.Sagas;

namespace OpenSpark.ApiGateway.Services
{
    public interface IActorSystemService
    {
        void StartSaga(ISagaCommand command);
        void SendDiscussionsCommand(ICommand command);
        void SendGroupsCommand(ICommand command);
        ActorSystem LocalSystem { get; set; }
    }

    public class ActorSystemService : IActorSystemService
    {
        private readonly IConfiguration _configuration;
        private readonly IActorRef _callbackActorRef;
        private readonly IActorRef _sagaManagerActorRef;

        public ActorSystemService(IActorFactoryService actorFactoryService, IConfiguration configuration)
        {
            _configuration = configuration;

            // Create local WebApiSystem
            var configString = File.ReadAllText("webapi-system.conf");
            var config = ConfigurationFactory.ParseString(configString);
            LocalSystem = ActorSystem.Create("WebApiSystem", config);

            // Create local actors for the system
            _callbackActorRef = actorFactoryService.CreateCallbackActor();
            _sagaManagerActorRef = actorFactoryService.CreateSagaManagerActor();
        }

        public ActorSystem LocalSystem { get; set; }

        public void StartSaga(ISagaCommand command)
        {
            _sagaManagerActorRef.Tell(command);
        }

        public void SendDiscussionsCommand(ICommand command)
        {
            var discussionsUrl = _configuration["akka:DiscussionsRemoteUrl"];
            var userManager = LocalSystem.ActorSelection($"{discussionsUrl}/UserManager");
            userManager.Tell(command, _callbackActorRef);
        }

        public void SendGroupsCommand(ICommand command)
        {
            var groupsUrl = _configuration["akka:GroupsRemoteUrl"];
            var groupManager = LocalSystem.ActorSelection($"{groupsUrl}/GroupManager");
            groupManager.Tell(command, _callbackActorRef);
        }

        public void Dispose()
        {
            LocalSystem?.Dispose();
        }
    }
}
