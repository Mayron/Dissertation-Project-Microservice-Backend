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
using OpenSpark.ApiGateway.Builders;
using OpenSpark.Shared.Commands.Sagas;

namespace OpenSpark.ApiGateway.Services
{
    public interface IActorSystemService
    {
        void ExecuteSaga(SagaContext context);
        void RegisterTransaction(Guid transactionId);
        void SendEmptyPayloadToClient(string clientCallbackMethod, string connectionId);
        void SendMultiQuery(MultiQueryContext multiQueryContext);
        void SendErrorToClient(string clientCallbackMethod, string connectionId, string errorMessage);
//        void SendRemoteMessage(int remoteSystemId, IMessage message, IActorRef callback = null);
        void SendRemoteQuery(QueryContext context, IActorRef callback = null);
        void SendRemoteSagaMessage(int remoteSystemId, IActorRef sagaActor, IMessage message);
        void SendSagaFailedMessage(Guid transactionId, string message);
        void SendSagaSucceededMessage(Guid transactionId, string message, IDictionary<string, string> args = null);
        void SubscribeToSaga(SubscribeToSagaTransactionCommand command);
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
                Props.Create(() => new SagaManagerActor(this, firestoreService)), "SagaManager");

            _projectsRemotePath = $"{configuration["akka:ProjectsRemoteUrl"]}/ProjectManager";
            _groupsRemotePath = $"{configuration["akka:GroupsRemoteUrl"]}/GroupManager";
            _discussionsRemotePath = $"{configuration["akka:DiscussionsRemoteUrl"]}/DiscussionManager";
        }

        /// <summary>
        /// Sends a query with metadata to a remote actor system.
        /// </summary>
        /// <param name="context">Contains information needed to send the query to the correct endpoint with metadata.</param>
        /// <param name="callback">By default, the callback will be CallbackActor but a
        /// Saga or MultiQueryHandler may need to set this to itself.</param>
        public void SendRemoteQuery(QueryContext context, IActorRef callback = null)
        {
            var remotePath = GetRemotePath(context.RemoteSystemId);
            var managerActorRef = _localSystem.ActorSelection(remotePath);
            managerActorRef.Tell(context.Query, callback ?? _callbackHandler);
        }

        public void SendRemoteSagaMessage(int remoteSystemId, IActorRef sagaActor, IMessage message)
        {
            var remotePath = GetRemotePath(remoteSystemId);
            var managerActorRef = _localSystem.ActorSelection(remotePath);
            managerActorRef.Tell(message, sagaActor);
        }

        public void SendMultiQuery(MultiQueryContext multiQueryContext) => 
            _multiQueryManager.Tell(multiQueryContext, _callbackHandler);

        public void ExecuteSaga(SagaContext context) => _sagaManager.Tell(context);

        public void RegisterTransaction(Guid transactionId) => _callbackHandler.Tell(transactionId);

        public void SubscribeToSaga(SubscribeToSagaTransactionCommand command) => _callbackHandler.Tell(command);

        public void SendErrorToClient(string clientCallbackMethod, string connectionId, string errorMessage) =>
            _callbackHandler.Tell(new PayloadEvent
            {
                MetaData = new QueryMetaData
                {
                    ConnectionId = connectionId,
                    Callback = clientCallbackMethod,
                },
                Errors = new[] { errorMessage }
            });

        public void SendEmptyPayloadToClient(string clientCallbackMethod, string connectionId) =>
            _callbackHandler.Tell(new PayloadEvent
            {
                MetaData = new QueryMetaData
                {
                    ConnectionId = connectionId,
                    Callback = clientCallbackMethod,
                },
                Payload = new string[] {}
            });

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

        private string GetRemotePath(int remoteSystemId)
        {
            return remoteSystemId switch
            {
                RemoteSystem.Projects => _projectsRemotePath,
                RemoteSystem.Groups => _groupsRemotePath,
                RemoteSystem.Discussions => _discussionsRemotePath,
                _ => throw new Exception($"Invalid remote system ID: {remoteSystemId}")
            };
        }

        public void Dispose()
        {
            _localSystem?.Dispose();
        }
    }
}