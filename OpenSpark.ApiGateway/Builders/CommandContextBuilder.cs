using OpenSpark.Shared;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Events.Payloads;
using System;
using OpenSpark.Shared.Domain;

namespace OpenSpark.ApiGateway.Builders
{
    public interface ICommandContextBuilder
    {
        ICommandContextBuilder ForRemoteSystem(int remoteSystemId);
        ICommandContextBuilder OnPayloadReceived(Action<PayloadEvent> onPayloadReceived);
        ICommandContextBuilder SetClientCallback(string connectionId, string clientCallbackMethod);
        CommandContext Build();
    }

    public class CommandContextBuilder : ICommandContextBuilder
    {
        private readonly User _user;
        private int _remoteSystemId;
        private readonly ICommand _command;
        private Action<PayloadEvent> _onPayloadReceived;
        private string _clientCallbackMethod;
        private string _connectionId;

        internal CommandContextBuilder(ICommand command, User user)
        {
            _command = command;
            _user = user;
        }

        public ICommandContextBuilder ForRemoteSystem(int remoteSystemId)
        {
            _remoteSystemId = remoteSystemId;
            return this;
        }

        public ICommandContextBuilder OnPayloadReceived(Action<PayloadEvent> onPayloadReceived)
        {
            _onPayloadReceived = onPayloadReceived;
            return this;
        }

        public ICommandContextBuilder SetClientCallback(string connectionId, string clientCallbackMethod)
        {
            _clientCallbackMethod = clientCallbackMethod;
            _connectionId = connectionId;
            return this;
        }

        public CommandContext Build()
        {
            _command.User = _user;
            _command.MetaData = new MetaData
            {
                CreatedAt = DateTime.Now,
                Id = Guid.NewGuid(),
                ConnectionId = _connectionId,
                Callback = _clientCallbackMethod
            };

            return new CommandContext
            {
                Command = _command,
                OnPayloadReceived = _onPayloadReceived,
                RemoteSystemId = _remoteSystemId,
            };
        }
    }
}