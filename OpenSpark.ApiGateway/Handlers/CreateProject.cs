﻿using Akka.Actor;
using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.SagaExecutionCommands;
using OpenSpark.Shared.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Handlers
{
    public class CreateProject
    {
        public class Command : IRequest<ValidationResult>
        {
            public NewProjectInputModel Model { get; }

            public Command(NewProjectInputModel model)
            {
                Model = model;
            }
        }

        public class Handler : IRequestHandler<Command, ValidationResult>
        {
            private readonly IActorSystemService _actorSystemService;
            private readonly User _user;

            public Handler(IActorSystemService actorSystemService, IHttpContextAccessor context)
            {
                _actorSystemService = actorSystemService;
                _user = context.GetFirebaseUser();
            }

            public Task<ValidationResult> Handle(Command command, CancellationToken cancellationToken)
            {
                if (_user == null)
                    return Task.FromResult(new ValidationResult(false, "Failed to validate user request"));

                var transactionId = Guid.NewGuid();
                _actorSystemService.SagaManager.Tell(new ExecuteCreateProjectSagaCommand
                {
                    SagaName = nameof(CreateProjectSagaActor),
                    TransactionId = transactionId,
                    Name = command.Model.Name,
                    About = command.Model.About,
                    Tags = command.Model.Tags,
                    User = _user
                });

                return Task.FromResult(new ValidationResult(true, transactionId.ToString()));
            }
        }
    }
}