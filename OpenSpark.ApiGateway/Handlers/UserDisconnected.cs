using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.Services.SDK;
using OpenSpark.Discussions.Commands;
using OpenSpark.Domain;

namespace OpenSpark.ApiGateway.Handlers
{
    public class UserDisconnected
    {
        public class Query : IRequest
        {
            public string ConnectionId { get; }

            public Query(string connectionId)
            {
                ConnectionId = connectionId;
            }
        }

        public class Handler : IRequestHandler<Query>
        {
            private readonly IRemoteActorSystemService _remoteActorSystemService;

            public Handler(IRemoteActorSystemService remoteActorSystemService)
            {
                _remoteActorSystemService = remoteActorSystemService;
            }

            public async Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var command = new DisconnectUserCommand
                {
                    ConnectionId = query.ConnectionId,
                };

                _remoteActorSystemService.Send(command);

                return await Unit.Task;
            }
        }
    }
}
