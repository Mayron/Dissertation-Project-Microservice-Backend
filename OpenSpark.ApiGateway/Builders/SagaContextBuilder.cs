using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Sagas;
using System;

namespace OpenSpark.ApiGateway.Builders
{
    public interface ISagaContextBuilder
    {
        ISagaContextBuilder SetClientCallback(string connectionId, string clientCallbackMethod);
        SagaContext Build();
    }

    public class SagaContextBuilder<T> : ISagaContextBuilder
    {
        private readonly User _user;
        private readonly ISagaExecutionCommand _command;
        private string _clientCallbackMethod;
        private string _connectionId;

        internal SagaContextBuilder(ISagaExecutionCommand command, User user)
        {
            _command = command;
            _user = user;
        }

        public ISagaContextBuilder SetClientCallback(string connectionId, string clientCallbackMethod)
        {
            _clientCallbackMethod = clientCallbackMethod;
            _connectionId = connectionId;
            return this;
        }

        public SagaContext Build()
        {
            var transactionId = Guid.NewGuid();

            _command.MetaData = new MetaData
            {
                ParentId = transactionId,
                CreatedAt = DateTime.Now,
                Id = Guid.NewGuid(),
                ConnectionId = _connectionId,
                Callback = _clientCallbackMethod
            };

            _command.User = _user;

            return new SagaContext
            {
                Command = _command,
                Id = transactionId,
                SagaName = typeof(T).Name
            };
        }
    }
}