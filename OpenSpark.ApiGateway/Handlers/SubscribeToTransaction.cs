using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using MediatR;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands;

namespace OpenSpark.ApiGateway.Handlers
{
    public class SubscribeToTransaction
    {
        public class Query : IRequest
        {
            public string ConnectionId { get; }
            public Guid TransactionId { get; }
            public string Callback { get; }

            public Query(string connectionId, Guid transactionId, string callback)
            {
                ConnectionId = connectionId;
                TransactionId = transactionId;
                Callback = callback;
            }
        }

        public class Handler : IRequestHandler<Query>
        {
            private readonly IActorSystemService _actorSystemService;

            public Handler(IActorSystemService actorSystemService)
            {
                _actorSystemService = actorSystemService;
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var command = new SubscribeToSagaTransactionCommand
                {
                    TransactionId = query.TransactionId,
                    ConnectionId = query.ConnectionId,
                    Callback = query.Callback
                };

                _actorSystemService.SubscribeToSaga(command);

                return Unit.Task;
            }
        }
    }
}