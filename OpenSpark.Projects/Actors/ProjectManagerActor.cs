using Akka.Actor;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Queries;
using System.Collections.Immutable;
using System.Linq;

namespace OpenSpark.Projects.Actors
{
    public class ProjectManagerActor : ReceiveActor
    {
        private IImmutableDictionary<string, IActorRef> _children;

        public ProjectManagerActor()
        {
            _children = ImmutableDictionary<string, IActorRef>.Empty;
            var createProjectPool = Context.ActorOf(CreateProjectActor.Props, "CreateProjectPool");
            var projectQueryPool = Context.ActorOf(ProjectQueryActor.Props, "ProjectQueryPool");

            Receive<DeleteProjectCommand>(command => ForwardByProjectId(command.ProjectId, command));

            Receive<ConnectAllProjectsCommand>(command =>
            {
                foreach (var projectId in command.ProjectIds)
                {
                    ForwardByProjectId(projectId, new ConnectProjectCommand
                    {
                        GroupId = command.GroupId,
                        ProjectId = projectId,
                        GroupVisibility = command.GroupVisibility,
                        User = command.User,
                    });
                }
            });

            // Pools
            Receive<CreateProjectCommand>(command => createProjectPool.Forward(command));
            Receive<ProjectDetailsQuery>(query => projectQueryPool.Forward(query));
            Receive<UserProjectsQuery>(query => projectQueryPool.Forward(query));

            Receive<Terminated>(terminated =>
            {
                Context.Unwatch(terminated.ActorRef);
                if (!_children.Any(u => u.Value.Equals(terminated.ActorRef))) return;

                _children = _children.Where(u => !u.Value.Equals(terminated.ActorRef)).ToImmutableDictionary();
            });
        }

        private void ForwardByProjectId(string projectId, IMessage message)
        {
            var actorRef = GetChildActor(projectId);
            actorRef.Forward(message);
        }

        private IActorRef GetChildActor(string projectId)
        {
            if (_children.ContainsKey(projectId))
                return _children[projectId];

            var groupActor = Context.Watch(Context.ActorOf(
                Props.Create<ProjectActor>(), $"Project-{projectId}"));

            _children = _children.Add(projectId, groupActor);

            return groupActor;
        }
    }
}