﻿using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Domain;
using OpenSpark.Shared.Queries;

namespace OpenSpark.ApiGateway.Builders
{
    public interface IMessageContextBuilderFactory
    {
        IQueryContextBuilder CreateQueryContext(IQuery query);

        IMultiQueryContextBuilder CreateMultiQueryContext<TH, TA>();

        ISagaContextBuilder CreateSagaContext<T>(ISagaExecutionCommand command);

        ICommandContextBuilder CreateCommandContext(ICommand command);
    }

    public class MessageContextBuilderFactory : IMessageContextBuilderFactory
    {
        private readonly User _user;

        public MessageContextBuilderFactory(IHttpContextAccessor contextAccessor) =>
            _user = contextAccessor.GetFirebaseUser();

        public IQueryContextBuilder CreateQueryContext(IQuery query) =>
            new QueryContextBuilder(query, _user);

        public IMultiQueryContextBuilder CreateMultiQueryContext<TH, TA>() =>
             new MultiQueryContextBuilder<TH, TA>(_user);

        public ISagaContextBuilder CreateSagaContext<T>(ISagaExecutionCommand command) =>
            new SagaContextBuilder<T>(command, _user);

        public ICommandContextBuilder CreateCommandContext(ICommand command) =>
            new CommandContextBuilder(command, _user);
    }
}