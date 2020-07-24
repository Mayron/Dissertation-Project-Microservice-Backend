using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.SagaExecutionCommands;
using OpenSpark.Shared.Queries;

namespace OpenSpark.ApiGateway.Services
{
    public interface IMessageContextBuilderService
    {
        IQueryContextBuilder CreateQueryContext(IQuery query);
        IMultiQueryContextBuilder CreateMultiQueryContext<TH, TA>();
        ISagaContextBuilder CreateSagaContext<T>(ISagaExecutionCommand command);
    }

    public class MessageContextBuilderService : IMessageContextBuilderService
    {
        private readonly User _user;

        public MessageContextBuilderService(IHttpContextAccessor contextAccessor) =>
            _user = contextAccessor.GetFirebaseUser();

        public IQueryContextBuilder CreateQueryContext(IQuery query) =>
            new QueryContextBuilder(query, _user);

        public IMultiQueryContextBuilder CreateMultiQueryContext<TH, TA>() =>
             new MultiQueryContextBuilder<TH, TA>(_user);

        public ISagaContextBuilder CreateSagaContext<T>(ISagaExecutionCommand command) => 
            new SagaContextBuilder<T>(command, _user);
    }
}