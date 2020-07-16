using System;
using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Configuration;
using OpenSpark.Shared.Commands;
using System.IO;
using OpenSpark.ApiGateway.Actors;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.Shared.Commands.Sagas.ExecutionCommands;

namespace OpenSpark.ApiGateway.Services
{
    public interface IActorSystemService
    {
        ActorSystem LocalSystem { get; }
        IActorRef SagaManager { get; }
        IActorRef CallbackHandler { get; }

        void SendDiscussionsCommand(ICommand command, IActorRef callback = null);
        void SendGroupsCommand(ICommand command, IActorRef callback = null);
        void SendProjectsCommand(ICommand command, IActorRef callback = null);
        IActorRef CreateSagaActor(ISagaExecutionCommand command);
    }

    public class ActorSystemService : IActorSystemService
    {
        private readonly IConfiguration _configuration;
        private readonly IEventEmitterService _eventEmitter;
        private readonly IFirestoreService _firestoreService;

        public ActorSystem LocalSystem { get; }
        public IActorRef SagaManager { get; }
        public IActorRef CallbackHandler { get; }

        public ActorSystemService(IConfiguration configuration, IEventEmitterService eventEmitter, IFirestoreService firestoreService)
        {
            _configuration = configuration;
            _eventEmitter = eventEmitter;
            _firestoreService = firestoreService;

            // Create local WebApiSystem
            var configString = File.ReadAllText("webapi-system.conf");
            var config = ConfigurationFactory.ParseString(configString);
            LocalSystem = ActorSystem.Create("WebApiSystem", config);

            // Create local actors for the system
            CallbackHandler = CreateCallbackActor();
            SagaManager = CreateSagaManagerActor();
        }

        public void SendDiscussionsCommand(ICommand command, IActorRef callback = null)
        {
            var discussionsUrl = _configuration["akka:DiscussionsRemoteUrl"];
            var userManager = LocalSystem.ActorSelection($"{discussionsUrl}/UserManager");
            userManager.Tell(command, callback ?? CallbackHandler);
        }

        public void SendGroupsCommand(ICommand command, IActorRef callback = null)
        {
            var groupsUrl = _configuration["akka:GroupsRemoteUrl"];
            var groupManager = LocalSystem.ActorSelection($"{groupsUrl}/GroupManager");
            groupManager.Tell(command, callback ?? CallbackHandler);
        }

        public void SendProjectsCommand(ICommand command, IActorRef callback = null)
        {
            var projectsUrl = _configuration["akka:ProjectsRemoteUrl"];
            var projectsManager = LocalSystem.ActorSelection($"{projectsUrl}/ProjectManager");
            projectsManager.Tell(command, callback ?? CallbackHandler);
        }

        public IActorRef CreateSagaActor(ISagaExecutionCommand command)
        {
            var actorName = $"{command.SagaName}-{command.TransactionId}";

            return command.SagaName switch
            {
                nameof(CreatePostSagaActor) => 
                    LocalSystem.ActorOf(
                        Props.Create(() => new CreatePostSagaActor(this, _eventEmitter)), actorName),

                nameof(CreateGroupSagaActor) => 
                    LocalSystem.ActorOf(
                        Props.Create(() => new CreateGroupSagaActor(this, _firestoreService)), actorName),

                _ => throw new Exception($"Failed to find SagaActor: {command.SagaName}"),
            };
        }

        public IActorRef CreateCallbackActor() =>
            LocalSystem.ActorOf(
                Props.Create(() => new CallbackActor(_eventEmitter)), "Callback");

        public IActorRef CreateSagaManagerActor() =>
            LocalSystem.ActorOf(
                Props.Create(() => new SagaManagerActor(this)), "SagaManager");

        public void Dispose()
        {
            LocalSystem?.Dispose();
        }
    }
}