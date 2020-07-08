using System;
using System.IO;
using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Configuration;
using OpenSpark.ApiGateway.Actors;
using OpenSpark.ApiGateway.Services.SDK;
using OpenSpark.Discussions.Commands;

namespace OpenSpark.ApiGateway.Services
{
    public class RemoteActorSystemService : IRemoteActorSystemService, IDisposable
    {
        private readonly ActorSystem _discussionsSystem;
        private readonly IActorRef _userManager;
        private readonly ActorSystem _webApiSystem;
        private readonly IActorRef _callbackActor;

        public RemoteActorSystemService(IEventEmitterService eventEmitter, IConfiguration configuration)
        {
            var configString = File.ReadAllText("actor-system.conf");
            var config = ConfigurationFactory.ParseString(configString);

            // local actor system to handle callbacks.
            _webApiSystem = ActorSystem.Create("WebApiSystem", config);
            _callbackActor = _webApiSystem.ActorOf(Props.Create(() => new WebApiCallbackActor(eventEmitter)), "WebApiCallback");

            // remote actor system
            _discussionsSystem = ActorSystem.Create("DiscussionsSystem", config);

            var discussionsRemoteUrl = configuration["akka:DiscussionsRemoteUrl"];
            _userManager = _discussionsSystem.ActorSelection($"{discussionsRemoteUrl}/UserManager")
                .ResolveOne(TimeSpan.FromMinutes(5))
                .Result;
        }

        /// <summary>
        /// Contact discussions system for new posts or popular posts (depending on if the user is authenticated).
        /// Posts are then sent via a message hub event asynchronously.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public void Send(FetchNewsFeedCommand command)
        {
            _userManager.Tell(command, _callbackActor);
        }

        public void Send(ConnectUserCommand command)
        {
            _userManager.Tell(command);
        }

        public void Send(DisconnectUserCommand command)
        {
            _userManager.Tell(command);
        }

        public void Dispose()
        {
            _discussionsSystem?.Dispose();
            _webApiSystem.Dispose();
        }
    }
}