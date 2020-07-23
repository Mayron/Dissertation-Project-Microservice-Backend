using Akka.Actor;
using OpenSpark.Shared.Commands.Projects;
using System;
using System.Collections.Immutable;

namespace OpenSpark.Projects.Actors
{
    public class ProjectActor : ReceiveActor
    {
        private IImmutableDictionary<string, IActorRef> _children;

        public ProjectActor()
        {
            SetReceiveTimeout(TimeSpan.FromMinutes(30));
            _children = ImmutableDictionary<string, IActorRef>.Empty;

            Receive<ConnectProjectCommand>(command =>
            {
                var name = $"ConnectProject-{command.ProjectId}";

                if (!_children.ContainsKey(name))
                {
                    var child = Context.ActorOf(Props.Create<ConnectProjectActor>(), name);
                    Context.Watch(child);
                    _children = _children.Add(name, child);
                }

                _children[name].Forward(command);
            });

            Receive<DeleteProjectCommand>(command =>
            {
                var actorRef = Context.ActorOf(
                    Props.Create<DeleteProjectActor>(), $"DeleteProject-{command.ProjectId}");

                actorRef.Forward(command);

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });

            Receive<Terminated>(terminated =>
            {
                Context.Unwatch(terminated.ActorRef);
                _children = _children.Remove(terminated.ActorRef.Path.Name);
            });
        }
    }
}