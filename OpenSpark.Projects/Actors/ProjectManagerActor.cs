using Akka.Actor;
using OpenSpark.Shared.Commands.Projects;
using System.Collections.Immutable;
using System.Linq;
using OpenSpark.Shared;
using OpenSpark.Shared.Queries;

namespace OpenSpark.Projects.Actors
{
    public class ProjectManagerActor : ReceiveActor
    {
        private IImmutableDictionary<string, IActorRef> _children;

        public ProjectManagerActor()
        {
            _children = ImmutableDictionary<string, IActorRef>.Empty;

            Receive<DeleteProjectCommand>(command => ForwardByProjectId(command.ProjectId, command));
            Receive<ProjectDetailsQuery>(query => ForwardByProjectId(query.ProjectId, query));

            Receive<ConnectAllProjectsCommand>(command =>
            {
                foreach (var projectId in command.ProjectIds)
                {
                    var projectActor = GetChildActor(projectId);
                    projectActor.Forward(new ConnectProjectCommand
                    {
                        GroupId = command.GroupId,
                        ProjectId = projectId,
                        GroupVisibility = command.GroupVisibility,
                        User = command.User,
                        TransactionId = command.TransactionId
                    });
                }
            });

            Receive<CreateProjectCommand>(command =>
            {
                var actorRef = Context.ActorOf(
                    Props.Create<CreateProjectActor>(), $"CreateProject-{command.TransactionId}");

                actorRef.Forward(command);
            });

            Receive<UserProjectsQuery>(query =>
            {
                var actorRef = Context.ActorOf(Props.Create<UserProjectsActor>());
                actorRef.Forward(query);
            });

            Receive<Terminated>(terminated =>
            {
                Context.Unwatch(terminated.ActorRef);
                if (!_children.Any(u => u.Value.Equals(terminated.ActorRef))) return;

                _children = _children.Where(u => !u.Value.Equals(terminated.ActorRef)).ToImmutableDictionary();
            });
        }

        private void ForwardByProjectId(string project, IMessage message)
        {
            var actorRef = GetChildActor(project);
            actorRef.Forward(message);
        }

        private IActorRef GetChildActor(string projectId)
        {
            if (_children.ContainsKey(projectId))
                return _children[projectId];

            var groupActor = Context.ActorOf(Props.Create<ProjectActor>(), $"Project-{projectId}");

            Context.Watch(groupActor);
            _children = _children.Add(projectId, groupActor);

            return groupActor;
        }
    }
}