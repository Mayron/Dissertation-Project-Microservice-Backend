using System;
using System.IO;
using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Configuration;
using OpenSpark.ApiGateway.Services.SDK;
using OpenSpark.Shared.Commands;

namespace OpenSpark.ApiGateway.Services
{
    public class RemoteActorSystemService : IRemoteActorSystemService
    {
        private readonly ILocalActorSystemService _localActorSystem;
        private readonly ActorSelection _userManager;

        public RemoteActorSystemService(IConfiguration configuration, ILocalActorSystemService localActorSystem)
        {
            _localActorSystem = localActorSystem;

            var discussionsUrl = configuration["akka:DiscussionsRemoteUrl"];
            _userManager = localActorSystem.LocalSystem.ActorSelection($"{discussionsUrl}/UserManager");
        }

        public void Send(ICommand command) => _userManager.Tell(command, _localActorSystem.CallbackActorRef);
    }
}