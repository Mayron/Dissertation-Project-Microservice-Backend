using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Configuration;
using OpenSpark.ApiGateway.Actors;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Commands.SagaExecutionCommands;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Events.Sagas;
using OpenSpark.Shared.Queries;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenSpark.ApiGateway.Services
{
    public interface IActorSystemService
    {
        void SendRemoteMessage(int remoteSystemId, IMessage message, IActorRef callback = null);

        void RegisterTransaction(Guid transactionId);

        void SendErrorToClient(string connectionId, string callback, string errorMessage);

        void SendPayloadToClient(string connectionId, string callback, object payload);

        void SendSagaFailedMessage(Guid transactionId, string message);

        void SendSagaSucceededMessage(Guid transactionId, string message, IDictionary<string, string> args = null);

        void SubscribeToSaga(SubscribeToSagaTransactionCommand command);

        void SendMultiQuery(MultiQuery query);

        void ExecuteSaga(ISagaExecutionCommand command);
    }

    public class ActorSystemService : IActorSystemService
    {
        private readonly ActorSystem _localSystem;
        private readonly IActorRef _callbackHandler;
        private readonly IActorRef _multiQueryManager;
        private readonly IActorRef _sagaManager;
        private readonly string _projectsRemotePath;
        private readonly string _groupsRemotePath;
        private readonly string _discussionsRemotePath;

        public ActorSystemService(
            IConfiguration configuration,
            IEventEmitterService eventEmitter,
            IFirestoreService firestoreService)
        {
            // Create local WebApiSystem
            var configString = File.ReadAllText("webapi-system.conf");
            var config = ConfigurationFactory.ParseString(configString);
            _localSystem = ActorSystem.Create("WebApiSystem", config);

            // Create local actors for the system
            _callbackHandler = _localSystem.ActorOf(
                Props.Create(() => new CallbackActor(eventEmitter)), "Callback");

            _multiQueryManager = _localSystem.ActorOf(
                Props.Create(() => new MultiQueryManagerActor(
                    this, _callbackHandler)), "MultiQueryManager");

            _sagaManager = _localSystem.ActorOf(
                Props.Create(() => new SagaManagerActor(this, eventEmitter, firestoreService)), "SagaManager");

            _projectsRemotePath = $"{configuration["akka:ProjectsRemoteUrl"]}/ProjectManager";
            _groupsRemotePath = $"{configuration["akka:GroupsRemoteUrl"]}/GroupManager";
            _discussionsRemotePath = $"{configuration["akka:DiscussionsRemoteUrl"]}/UserManager";
        }

        public void SendRemoteMessage(int remoteSystemId, IMessage message, IActorRef callback = null)
        {
            var remoteActorPath = remoteSystemId switch
            {
                RemoteSystem.Groups => _groupsRemotePath,
                RemoteSystem.Discussions => _discussionsRemotePath,
                RemoteSystem.Projects => _projectsRemotePath,
                _ => throw new Exception($"Invalid remote system ID: {remoteSystemId}")
            };

            var managerActorRef = _localSystem.ActorSelection(remoteActorPath);
            managerActorRef.Tell(message, callback ?? _callbackHandler);
        }

        public void SendMultiQuery(MultiQuery query) => _multiQueryManager.Tell(query, _callbackHandler);

        public void RegisterTransaction(Guid transactionId) => _callbackHandler.Tell(transactionId);

        public void SubscribeToSaga(SubscribeToSagaTransactionCommand command) => _callbackHandler.Tell(command);

        public void ExecuteSaga(ISagaExecutionCommand command) => _sagaManager.Tell(command);

        public void SendErrorToClient(string connectionId, string callback, string errorMessage)
        {
            _callbackHandler.Tell(new PayloadEvent
            {
                ConnectionId = connectionId,
                Callback = callback,
                Errors = new[] { errorMessage }
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