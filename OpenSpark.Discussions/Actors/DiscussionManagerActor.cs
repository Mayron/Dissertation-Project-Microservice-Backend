using Akka.Actor;
using OpenSpark.Shared.Events;
using OpenSpark.Shared.Queries;
using System.Collections.Generic;
using System.Linq;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Commands.Discussions;

namespace OpenSpark.Discussions.Actors
{
    public class DiscussionManagerActor : ReceiveActor
    {
        private readonly IDictionary<string, IActorRef> _postQueryActors;

        public DiscussionManagerActor()
        {
            _postQueryActors = new Dictionary<string, IActorRef>();
            var createPostPool = Context.ActorOf(CreatePostActor.Props, "CreatePostPool");
            var commentsPool = Context.ActorOf(CommentsActor.Props, "CommentsPool");

            // Pools
            Receive<CreatePostCommand>(command => createPostPool.Forward(command));
            Receive<CreateCommentCommand>(command => commentsPool.Forward(command));
            Receive<CommentsQuery>(query => commentsPool.Forward(query));
            Receive<ChangeVoteCommand>(command => commentsPool.Forward(command));

            // PostQuery queries
            Receive<NewsFeedQuery>(ForwardByConnectionId);
            Receive<GroupPostsQuery>(ForwardByConnectionId);

            Receive<Terminated>(terminated =>
            {
                foreach (var (key, value) in _postQueryActors.Where(kv => kv.Value.Equals(terminated.ActorRef)).ToList())
                {
                    Context.Unwatch(value);
                    _postQueryActors.Remove(key);
                }
            });

            Receive<DisconnectedEvent>(@event =>
            {
                if (!_postQueryActors.ContainsKey(@event.ConnectionId)) return;
                var child = _postQueryActors[@event.ConnectionId];
                Context.Stop(child);
            });
        }

        private void ForwardByConnectionId(IQuery query)
        {
            var connectionId = query.MetaData.ConnectionId;
            if (_postQueryActors.ContainsKey(connectionId))
            {
                _postQueryActors[connectionId].Forward(query);
                return;
            }

            var child = Context.Watch(Context.ActorOf<PostQueryActor>($"PostQuery-{connectionId}"));
            _postQueryActors.Add(connectionId, child);

            child.Forward(query);
        }
    }
}