﻿using System;
using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Queries;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.Shared;

namespace OpenSpark.ApiGateway.Handlers
{
    public class FetchGroupProjects
    {
        public class Query : IRequest<Unit>
        {
            public string GroupId { get; }
            public string ConnectionId { get; }
            public string Callback { get; }

            public Query(string groupId, string connectionId, string callback)
            {
                GroupId = groupId;
                ConnectionId = connectionId;
                Callback = callback;
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
                // TODO: 1- Need to use GroupProjectsQuery to get all projects and
                // TODO: 2- then use ProjectDetailsQuery with RetrieveProjectNameOnly true
                // TODO: 3- This will need a "Saga"-Like FSM actor, but what to call it?

                return Unit.Task;
            }
        }
    }
}