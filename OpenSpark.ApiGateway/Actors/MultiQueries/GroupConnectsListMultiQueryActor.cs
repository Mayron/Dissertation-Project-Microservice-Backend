using Akka.Actor;
using OpenSpark.Shared;
using OpenSpark.Shared.Events;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace OpenSpark.ApiGateway.Actors.MultiQueries
{
    public class GroupConnectsListMultiQueryActor : ReceiveActor
    {
        private readonly MultiQuery _query;

        public GroupConnectsListMultiQueryActor(MultiQuery query, IActorRef callback)
        {
            _query = query;

            SetReceiveTimeout(TimeSpan.FromSeconds(query.TimeOutInSeconds * 2));

            Receive<MultiPayloadEvent>(@event =>
            {
                if (@event.MultiQueryId != query.Id)
                    throw new Exception($"Invalid multi query Id. Expected {query.Id} but got {@event.MultiQueryId}");

                var payload = AggregatePayload(@event.Payloads);
                callback.Tell(payload);

                Context.Stop(Self);
            });

            Receive<ReceiveTimeout>(r =>
            {
                callback.Tell(new PayloadEvent
                {
                    ConnectionId = query.ConnectionId,
                    Callback = query.Callback,
                    Errors = new [] { "Request timed out" }
                });
                Context.Stop(Self);
            });
        }

        private PayloadEvent AggregatePayload(IImmutableDictionary<Guid, IPayloadEvent> payloads)
        {
            var successful = new List<object>();
            var errors = new List<string>();

            foreach (var (_, @event) in payloads) // not sure what to do with key yet
            {
                switch (@event)
                {
                    case PayloadEvent payloadEvent when payloadEvent.Payload != null:
                        successful.Add(payloadEvent.Payload);
                        break;

                    case PayloadEvent errorEvent when errorEvent.Errors != null:
                        errors.AddRange(errorEvent.Errors);
                        break;
                }
            }

            if (errors.Count > 0)
            {
                return new PayloadEvent
                {
                    ConnectionId = _query.ConnectionId,
                    Callback = _query.Callback,
                    Errors = errors.ToArray()
                };
            }

            if (successful.Count != _query.Queries.Count)
            {
                Console.WriteLine($"Corrupt multi-query data. Expected {_query.Queries.Count} results but got {successful.Count} with no errors.");
                return new PayloadEvent
                {
                    ConnectionId = _query.ConnectionId,
                    Callback = _query.Callback,
                    Errors = new[] { "Oops! Something went wrong while retrieving data." }
                };
            }

            var project = successful
                .Where(r => r is ProjectDetailsViewModel)
                .Cast<ProjectDetailsViewModel>()
                .Single();

            var groups = successful
                .Where(s => s is UserGroupsViewModel)
                .Cast<UserGroupsViewModel>()
                .ToList();

            var results = new List<ConnectionViewModel>();

            foreach (var group in groups)
            {
                var (canConnect, error) =
                    VisibilityHelper.CanProjectConnectToGroup(project.Visibility, group.Visibility);

                results.Add(new ConnectionViewModel
                {
                    Visibility = group.Visibility,
                    Id = group.Id,
                    Name = group.Name,
                    Available = canConnect,
                    NotAvailableMessage = error
                });
            }

            return new PayloadEvent
            {
                ConnectionId = _query.ConnectionId,
                Callback = _query.Callback,
                Payload = results
            };
        }
    }
}