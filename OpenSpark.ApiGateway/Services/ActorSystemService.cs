using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Configuration;
using OpenSpark.ApiGateway.Actors;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.SagaExecutionCommands;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Events.Sagas;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenSpark.ApiGateway.Services
{
    public interface IActorSystemService
    {
        IActorRef SagaManager { get; }

        void SendDiscussionsMessage(IMessage message, IActorRef callback = null);

        void SendGroupsMessage(IMessage message, IActorRef callback = null);

        void SendProjectsMessage(IMessage message, IActorRef callback = null);

        IActorRef CreateSagaActor(ISagaExecutionCommand command);

        void RegisterTransaction(Guid transactionId);

        void SendErrorToClient(string connectionId, string callback, string errorMessage);

        void SendPayloadToClient(string connectionId, string callback, object payload);

        void SendSagaFailedMessage(Guid transactionId, string message);

        void SendSagaSucceededMessage(Guid transactionId, string message, IDictionary<string, string> args = null);
    }

    public class ActorSystemService : IActorSystemService
    {
        private readonly IConfiguration _configuration;
        private readonly IEventEmitterService _eventEmitter;
        private readonly IFirestoreService _firestoreService;
        private readonly ActorSystem _localSystem;
        private readonly IActorRef _callbackHandler;

        public IActorRef SagaManager { get; }

        public ActorSystemService(IConfiguration configuration, IEventEmitterService eventEmitter,
            IFirestoreService firestoreService)
        {
            _configuration = configuration;
            _eventEmitter = eventEmitter;
            _firestoreService = firestoreService;

            // Create local WebApiSystem
            var configString = File.ReadAllText("webapi-system.conf");
            var config = ConfigurationFactory.ParseString(configString);
            _localSystem = ActorSystem.Create("WebApiSystem", config);

            // Create local actors for the system
            _callbackHandler = _localSystem.ActorOf(
                Props.Create(() => new CallbackActor(_eventEmitter)), "Callback");

            SagaManager = _localSystem.ActorOf(
                Props.Create(() => new SagaManagerActor(this)), "SagaManager");
        }

        public void SendDiscussionsMessage(IMessage message, IActorRef callback = null)
        {
            var discussionsUrl = _configuration["akka:DiscussionsRemoteUrl"];
            var userManager = _localSystem.ActorSelection($"{discussionsUrl}/UserManager");
            userManager.Tell(message, callback ?? _callbackHandler);
        }

        public void SendGroupsMessage(IMessage message, IActorRef callback = null)
        {
            var groupsUrl = _configuration["akka:GroupsRemoteUrl"];
            var groupManager = _localSystem.ActorSelection($"{groupsUrl}/GroupManager");
            groupManager.Tell(message, callback ?? _callbackHandler);
        }

        public void SendProjectsMessage(IMessage message, IActorRef callback = null)
        {
            var projectsUrl = _configuration["akka:ProjectsRemoteUrl"];
            var projectsManager = _localSystem.ActorSelection($"{projectsUrl}/ProjectManager");
            projectsManager.Tell(message, callback ?? _callbackHandler);
        }

        public IActorRef CreateSagaActor(ISagaExecutionCommand command)
        {
            var actorName = $"{command.SagaName}-{command.TransactionId}";

            return command.SagaName switch
            {
                nameof(CreatePostSagaActor) =>
                _localSystem.ActorOf(
                    Props.Create(() => new CreatePostSagaActor(this, _eventEmitter)), actorName),

                nameof(CreateGroupSagaActor) =>
                _localSystem.ActorOf(
                    Props.Create(() => new CreateGroupSagaActor(this, _firestoreService)), actorName),

                nameof(CreateProjectSagaActor) =>
                _localSystem.ActorOf(
                    Props.Create(() => new CreateProjectSagaActor(this, _firestoreService)), actorName),

                _ => throw new Exception($"Failed to find SagaActor: {command.SagaName}"),
            };
        }

        public void RegisterTransaction(Guid transactionId) => _callbackHandler.Tell(transactionId);

        public void SendErrorToClient(string connectionId, string callback, string errorMessage)
        {
            _callbackHandler.Tell(new PayloadEvent
            {
                ConnectionId = connectionId,
                Callback = callback,
                Error = errorMessage
            });
        }

        public void SendPayloadToClient(string connectionId, string callback, object payload)
        {
            _callbackHandler.Tell(new PayloadEvent
            {
                ConnectionId = connectionId,
                Callback = callback,
                Payload = payload
            });
        }

        public void SendSagaFailedMessage(Guid transactionId, string message) =>
            SendSagaMessage(transactionId, message, false);

        public void SendSagaSucceededMessage(Guid transactionId, string message, IDictionary<string, string> args = null) =>
            SendSagaMessage(transactionId, message, true, args);

        private void SendSagaMessage(Guid transactionId, string message, bool success, IDictionary<string, string> args = null)
        {
            _callbackHandler.Tell(new SagaFinishedEvent
            {
                TransactionId = transactionId,
                Message = message,
                Success = success,
                Args = args
            });
        }

        public void Dispose()
        {
            _localSystem?.Dispose();
        }
    }
}