using OpenSpark.Domain;
using OpenSpark.Shared.Commands.SagaExecutionCommands;
using System;

namespace OpenSpark.ApiGateway.Builders
{
    public interface ISagaContextBuilder
    {
        SagaContext Build();
    }

    public class SagaContextBuilder<T> : ISagaContextBuilder
    {
        private readonly User _user;
        private readonly ISagaExecutionCommand _command;

        internal SagaContextBuilder(ISagaExecutionCommand command, User user)
        {
            _command = command;
            _user = user;
        }

        public SagaContext Build()
        {
            var transactionId = Guid.NewGuid();
            _command.TransactionId = transactionId;
            _command.User = _user;
            _command.CreatedAt = DateTime.Now;

            return new SagaContext
            {
                Command = _command,
                TransactionId = transactionId,
                SagaName = typeof(T).Name
            };
        }
    }
}