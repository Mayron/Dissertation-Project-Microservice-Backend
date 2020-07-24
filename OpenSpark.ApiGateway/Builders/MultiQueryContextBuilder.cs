﻿using System;
using System.Collections.Generic;
using OpenSpark.Domain;
using OpenSpark.Shared.Queries;

namespace OpenSpark.ApiGateway.Builders
{
    public interface IMultiQueryContextBuilder
    {
        IMultiQueryContextBuilder SetClientCallback(string clientCallbackMethod, string connectionId);
        IMultiQueryContextBuilder AddQuery(IQuery query, int remoteSystemId);
        IMultiQueryContextBuilder SetCustomTimeout(int timeout);
        MultiQueryContext Build();
    }

    public class MultiQueryContextBuilder<TH, TA> : IMultiQueryContextBuilder
    {
        private readonly User _user;
        private readonly string _aggregator;
        private readonly string _handler;
        private string _clientCallbackMethod;
        private string _connectionId;
        private readonly List<QueryContext> _queries;
        private readonly Guid _multiQueryId;
        private int _timeout;

        internal MultiQueryContextBuilder(User user)
        {
            _multiQueryId = Guid.NewGuid();
            _user = user;
            _handler = typeof(TH).Name;
            _aggregator = typeof(TA).Name;
            _queries = new List<QueryContext>();
            _timeout = 8; // default timeout in seconds
        }

        public IMultiQueryContextBuilder SetClientCallback(string clientCallbackMethod, string connectionId)
        {
            _clientCallbackMethod = clientCallbackMethod;
            _connectionId = connectionId;
            return this;
        }

        public IMultiQueryContextBuilder AddQuery(IQuery query, int remoteSystemId)
        {
            query.MetaData = new QueryMetaData
            {
                MultiQueryId = _multiQueryId,
                QueryId = Guid.NewGuid()
            };

            _queries.Add(new QueryContext
            {
                Query = query,
                RemoteSystemId = remoteSystemId,
            });

            return this;
        }

        public IMultiQueryContextBuilder SetCustomTimeout(int timeout)
        {
            _timeout = timeout;
            return this;
        }

        public MultiQueryContext Build()
        {
            var createdAt = DateTime.Now;

            foreach (var query in _queries)
                query.Query.MetaData.CreatedAt = createdAt;

            return new MultiQueryContext
            {
                Id = _multiQueryId,
                CreatedAt = createdAt,
                User = _user,
                ConnectionId = _connectionId,
                Callback = _clientCallbackMethod,
                Queries = _queries,
                Handler = _handler,
                Aggregator = _aggregator,
                TimeoutInSeconds = _timeout,
            };
        }
    }
}