using System.IO;
using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Configuration;
using OpenSpark.ApiGateway.Actors;
using OpenSpark.Shared.Commands;

namespace OpenSpark.ApiGateway.Services
{
    public interface IActorSystemService
    {
        void StartAddPostSaga(CreateUserPostRequestCommand createUserPostRequestCommand);
        void SendDiscussionsCommand(ICommand command);
        void SendGroupsCommand(ICommand command);
    }

    public class ActorSystemService : IActorSystemService
    {
        private readonly IConfiguration _configuration;
        private readonly ActorSystem _localSystem;
        private readonly IActorRef _callbackActorRef;

        public ActorSystemService(IEventEmitterService eventEmitter, IConfiguration configuration)
        {
            _configuration = configuration;
            // Create local WebApiSystem
            var configString = File.ReadAllText("webapi-system.conf");
            var config = ConfigurationFactory.ParseString(configString);
            _localSystem = ActorSystem.Create("WebApiSystem", config);

            // Create local actors for the system
            _callbackActorRef = _localSystem.ActorOf(Props.Create(() => new CallbackActor(eventEmitter)), "Callback");
        }

        public void StartAddPostSaga(CreateUserPostRequestCommand createUserPostRequestCommand)
        {
            throw new System.NotImplementedException();
        }

        public void SendDiscussionsCommand(ICommand command)
        {
            var discussionsUrl = _configuration["akka:DiscussionsRemoteUrl"];
            var userManager = _localSystem.ActorSelection($"{discussionsUrl}/UserManager");
            userManager.Tell(command, _callbackActorRef);
        }

        public void SendGroupsCommand(ICommand command)
        {
            var groupsUrl = _configuration["akka:GroupsRemoteUrl"];
            var userManager = _localSystem.ActorSelection($"{groupsUrl}/UserManager");
            userManager.Tell(command, _callbackActorRef);
        }

        public void Dispose()
        {
            _localSystem?.Dispose();
        }
    }
}
