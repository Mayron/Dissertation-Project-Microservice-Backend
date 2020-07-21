using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Actors.MultiQueries;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Queries;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Handlers
{
    public class FetchGroupConnectionsList
    {
        public class Query : IRequest<Unit>
        {
            public string ConnectionId { get; }
            public string Callback { get; }
            public string ProjectId { get; }

            public Query(string connectionId, string callback, string projectId)
            {
                ConnectionId = connectionId;
                Callback = callback;
                ProjectId = projectId;
            }
        }

        public class Handler : IRequestHandler<Query, Unit>
        {
            private readonly IActorSystemService _actorSystemService;
            private readonly User _user;

            public Handler(IActorSystemService actorSystemService, IHttpContextAccessor context)
            {
                _actorSystemService = actorSystemService;
                _user = context.GetFirebaseUser();
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                if (_user == null || _user.Projects.Count == 0)
                {
                    _actorSystemService.SendErrorToClient(query.ConnectionId, query.Callback, "Unauthorized");
                    return Unit.Task;
                }

                if (_user.Groups.Count == 0)
                {
                    _actorSystemService.SendPayloadToClient(query.ConnectionId, query.Callback, _user.Groups);
                    return Unit.Task;
                }

                _actorSystemService.SendMultiQuery(
                    new MultiQuery
                    {
                        User = _user,
                        ConnectionId = query.ConnectionId,
                        Callback = query.Callback,
                        TimeOutInSeconds = 600,
                        MultiQueryName = nameof(GroupConnectsListMultiQueryActor),
                        Queries = new List<QueryContext>
                        {
                            new QueryContext
                            {
                                RemoteSystemId = RemoteSystem.Groups,
                                Query = new UserGroupsQuery
                                {
                                    User = _user,
                                    OwnedGroups = true
                                },
                            },
                            new QueryContext
                            {
                                RemoteSystemId = RemoteSystem.Projects,
                                Query = new ProjectDetailsQuery
                                {
                                    User = _user,
                                    ProjectId = query.ProjectId
                                }
                            }
                        }
                    });

                return Unit.Task;
            }
        }
    }
}