using System;
using OpenSpark.Domain;
using OpenSpark.Shared.Queries;

namespace OpenSpark.ApiGateway.Builders
{
    public interface IQueryContextBuilder
    {
        IQueryContextBuilder ForRemoteSystem(int remoteSystemId);
        IQueryContextBuilder SetClientCallback(string connectionId, string clientCallbackMethod);
        IQueryContextBuilder SetMultiQueryId(Guid id);
        QueryContext Build();
    }

    public class QueryContextBuilder : IQueryContextBuilder
    {
        private readonly User _user;
        private int _remoteSystemId;
        private string _connectionId;
        private string _clientCallbackMethod;
        private readonly IQuery _query;
        private Guid _multiQueryId;

        internal QueryContextBuilder(IQuery query, User user)
        {
            _query = query;
            _user = user;
        }

        public IQueryContextBuilder ForRemoteSystem(int remoteSystemId)
        {
            _remoteSystemId = remoteSystemId;
            return this;
        }

        public IQueryContextBuilder SetClientCallback(string clientCallbackMethod, string connectionId)
        {
            _clientCallbackMethod = clientCallbackMethod;
            _connectionId = connectionId;
            return this;
        }

        public IQueryContextBuilder SetMultiQueryId(Guid multiQueryId)
        {
            _multiQueryId = multiQueryId;
            return this;
        }

        public QueryContext Build()
        {
            _query.User = _user;
            _query.MetaData = new QueryMetaData
            {
                MultiQueryId = _multiQueryId,
                ConnectionId = _connectionId,
                Callback = _clientCallbackMethod,
                CreatedAt = DateTime.Now,
                QueryId = Guid.NewGuid()
            };

            return new QueryContext
            {
                Query = _query,
                RemoteSystemId = _remoteSystemId,
            };
        }
    }
}