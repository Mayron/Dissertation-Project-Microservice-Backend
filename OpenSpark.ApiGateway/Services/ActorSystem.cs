using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Configuration;
using OpenSpark.ApiGateway.Actors;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.Shared;
using OpenSpark.Shared.Events;
using OpenSpark.Shared.Events.Payloads;
using System;
using System.IO;

namespace OpenSpark.ApiGateway.Services
{
    public interface IActorSystemService
    {
        void ExecuteCommand(CommandContext context, IActorRef sender = null);

        void ExecuteSaga(SagaContext context);

        void SendMultiQuery(MultiQueryContext multiQueryContext);

        void SendQuery(QueryContext context, IActorRef sender = null);

        // Saga's do not have a reference to HttpContext and cannot use a MessageContextBuilder
        void SendSagaMessage(IMessage message, int remoteSystemId, Guid transactionId, IActorRef sender);

        // Saga helper function to send successful payload to client
        void SendPayloadToClient(MetaData metaData, object payload);

        // Saga helper function to send empty payload to client
        void SendEmptyPayloadToClient(MetaData data);

        // Saga helper function to send errors to client
        void SendErrorsToClient(MetaData data, string[] errors);

        // Saga helper function to send error to client
        void SendErrorToClient(MetaData data, string error);

        void PublishDisconnection(string queryConnectionId);
    }

    public class ActorSystemService : IActorSystemService
    {
        private readonly IActorRef _callbackHandler;
        private readonly string _discussionsRemotePath;
        private readonly string _teamsRemotePath;
        private readonly IEventEmitter _eventEmitter;
        private readonly string _groupsRemotePath;
        private readonly ActorSystem _localSystem;
        private readonly IActorRef _multiQueryManager;
        private readonly string _projectsRemotePath;
        private readonly IActorRef _sagaManager;

        public ActorSystemService(
            IConfiguration configuration,
            IEventEmitter eventEmitter,
            IFirestoreService firestoreService)
        {
            // Create local WebApiSystem
            var configString = File.ReadAllText("webapi-system.conf");
            var config = ConfigurationFactory.ParseString(configString);

            _localSystem = ActorSystem.Create("WebApiSystem", config);
            _eventEmitter = eventEmitter;

            // Create local actors for the system
            _callbackHandler = _localSystem.ActorOf(
                Props.Create(() => new CallbackHandlerActor(eventEmitter)), "Callback");

            _multiQueryManager = _localSystem.ActorOf(
                Props.Create(() => new MultiQueryManagerActor(
                    this, _callbackHandler, firestoreService)), "MultiQueryManager");

            _sagaManager = _localSystem.ActorOf(
                Props.Create(() => new SagaManagerActor(this, firestoreService)), "SagaManager");

            _projectsRemotePath = $"{configuration["akka:ProjectsRemoteUrl"]}/ProjectManager";
            _groupsRemotePath = $"{configuration["akka:GroupsRemoteUrl"]}/GroupManager";
            _discussionsRemotePath = $"{configuration["akka:DiscussionsRemoteUrl"]}/DiscussionManager";
            _teamsRemotePath = $"{configuration["akka:TeamsRemoteUrl"]}/TeamManager";
        }

        public void ExecuteCommand(CommandContext context, IActorRef sender = null)
        {
            var remotePath = GetRemotePath(context.RemoteSystemId);

            if (context.OnPayloadReceived != null)
                _eventEmitter.RegisterCallback(context.Command.MetaData.Id, context.OnPayloadReceived);

            var managerActorRef = _localSystem.ActorSelection(remotePath);
            managerActorRef.Tell(context.Command, sender ?? _callbackHandler);
        }

        public void ExecuteSaga(SagaContext context) => _sagaManager.Tell(context);

        /// <summary>
        /// Send a query or command from a Saga actor to a remote destination with the Saga as the sender.
        /// A Saga does not have access to HttpContext and so it cannot use a MessageContextBuilder.
        /// The connectionId and client callback method is stored in the saga's state and so the message does
        /// not require this in its meta-data.
        /// </summary>
        public void SendSagaMessage(IMessage message, int remoteSystemId, Guid transactionId, IActorRef sender)
        {
            message.MetaData = new MetaData
            {
                CreatedAt = DateTime.Now,
                ParentId = transactionId,
                Id = Guid.NewGuid()
            };

            var remotePath = GetRemotePath(remoteSystemId);
            var managerActorRef = _localSystem.ActorSelection(remotePath);
            managerActorRef.Tell(message, sender);
        }

        // Saga helper function to send successful payload to client
        public void SendPayloadToClient(MetaData data, object payload) =>
            _eventEmitter.BroadcastToClient(new PayloadEvent { MetaData = data, Payload = payload });

        // Saga helper function to send empty payload to client
        public void SendEmptyPayloadToClient(MetaData data) =>
            _eventEmitter.BroadcastToClient(new PayloadEvent { MetaData = data, Payload = new string[] { } });

        // Saga helper function to send errors to client
        public void SendErrorsToClient(MetaData data, string[] errors) =>
            _eventEmitter.BroadcastToClient(new PayloadEvent { MetaData = data, Errors = errors });

        // Saga helper function to send error to client
        public void SendErrorToClient(MetaData data, string error) =>
            _eventEmitter.BroadcastToClient(new PayloadEvent { MetaData = data, Errors = new []{ error } });

        public void SendMultiQuery(MultiQueryContext multiQueryContext) => _multiQueryManager.Tell(multiQueryContext);

        /// <summary>
        /// Sends a query with metadata to a remote actor system.
        /// </summary>
        /// <param name="context">Contains information needed to send the query to the correct endpoint with metadata.</param>
        /// <param name="sender">By default, the sender will be CallbackActor but a
        /// Saga or MultiQueryHandler may need to set this to itself.</param>
        public void SendQuery(QueryContext context, IActorRef sender = null)
        {
            var remotePath = GetRemotePath(context.RemoteSystemId);

            if (context.OnPayloadReceived != null)
                _eventEmitter.RegisterCallback(context.Query.MetaData.Id, context.OnPayloadReceived);

            var managerActorRef = _localSystem.ActorSelection(remotePath);
            managerActorRef.Tell(context.Query, sender ?? _callbackHandler);
        }

        // TODO: Change this to use a publish-subscribe pattern
        public void PublishDisconnection(string connectionId)
        {
            var remotePath = GetRemotePath(RemoteSystem.Discussions);
            var managerActorRef = _localSystem.ActorSelection(remotePath);
            managerActorRef.Tell(new DisconnectedEvent
            {
                ConnectionId = connectionId
            });
        }

        private string GetRemotePath(int remoteSystemId)
        {
            return remoteSystemId switch
            {
                RemoteSystem.Projects => _projectsRemotePath,
                RemoteSystem.Groups => _groupsRemotePath,
                RemoteSystem.Discussions => _discussionsRemotePath,
                RemoteSystem.Teams => _teamsRemotePath,
                _ => throw new Exception($"Invalid remote system ID: {remoteSystemId}")
            };
        }
    }
}