using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Commands.Projects;

namespace OpenSpark.Projects.Actors
{
    public class ProjectManagerActor : ReceiveActor
    {
        private IImmutableDictionary<string, IActorRef> _children;

        public ProjectManagerActor()
        {
            _children = ImmutableDictionary<string, IActorRef>.Empty;

            Receive<ConnectAllProjectsCommand>(command =>
            {
                foreach (var projectId in command.ProjectIds)
                {
                    var projectActor = GetProjectChildActor(projectId);
                    projectActor.Forward(new ConnectProjectCommand
                    {
                        GroupId = command.GroupId,
                        ProjectId = projectId
                    });
                }
            });

            Receive<Terminated>(terminated =>
            {
                Context.Unwatch(terminated.ActorRef);
                if (!_children.Any(u => u.Value.Equals(terminated.ActorRef))) return;

                _children = _children.Where(u => !u.Value.Equals(terminated.ActorRef)).ToImmutableDictionary();
            });
        }


        private IActorRef GetProjectChildActor(string id)
        {
            if (_children.ContainsKey(id))
                return _children[id];

            var groupActor = Context.ActorOf(
                Props.Create(() => new ProjectActor(id)), $"Project-{id}");

            Context.Watch(groupActor);
            _children = _children.Add(id, groupActor);

            return groupActor;
        }
    }
}