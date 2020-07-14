using System;
using Akka.Actor;
using OpenSpark.ApiGateway.Models.StateData;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Events;

namespace OpenSpark.ApiGateway.Actors.Sagas
{
    public class AddPostSaga : FSM<AddPostSaga.State, IStateData>
    {
        private readonly IActorSystemService _actorSystemService;
        private readonly IEventEmitterService _eventEmitter;

        public enum State
        {
            Idle,
            VerifyingUser,
            AddingPost,
            Success,
            Error
        }

        public AddPostSaga(IActorSystemService actorSystemService, IEventEmitterService eventEmitter)
        {
            _actorSystemService = actorSystemService;
            _eventEmitter = eventEmitter;

            StartWith(State.Idle, Uninitialized.Instance);

            When(State.Idle, ev =>
            {
                if (ev.FsmEvent is CreateUserPostRequestCommand command && ev.StateData is Uninitialized)
                {
                    _actorSystemService.SendGroupsCommand(new VerifyUserPostRequestCommand());

                    return GoTo(State.VerifyingUser).Using(new VerifyingUserData());
                }

                return null;
            });

            When(State.VerifyingUser, state =>
            {
                switch (state.FsmEvent)
                {
                    case UserVerifiedEvent _ when state.StateData is VerifyingUserData data:
                        _actorSystemService.SendDiscussionsCommand(new AddPostCommand());
                        return GoTo(State.AddingPost).Using(new AddingPostData());

                    case UserVerificationFailedEvent _ when state.StateData is VerifyingUserData data:
                        return GoTo(State.Error).Using(
                            new Error("User does not have permission to post to group"));

                    case StateTimeout _:
                        return GoTo(State.Error).Using(
                            new Error("Request timed out while verifying user."));

                    default:
                        GoTo(State.Error).Using(
                            new Error($"Received unknown event during VerifyingUser state: {state.FsmEvent}"));
                        break;
                }

                return null;
            }, TimeSpan.FromSeconds(5)); // generates StateTimeout message if no response in 5 seconds

            When(State.AddingPost, state =>
            {
                switch (state.FsmEvent)
                {
                    case PostAddedEvent ev when state.StateData is AddingPostData data:
                        _eventEmitter.ReceivedEvent(ev);
                        return GoTo(State.Success).Using(new Success("Success!"));

                    case StateTimeout _:
                        return GoTo(State.Error).Using(
                            new Error("Request timed out while adding post."));

                    default:
                        GoTo(State.Error).Using(
                            new Error($"Received unknown event during AddingPost state: {state.FsmEvent}"));
                        break;
                }

                return null;
            }, TimeSpan.FromSeconds(5)); // generates StateTimeout message if no response in 5 seconds

            // unhandled state
            WhenUnhandled(state => GoTo(State.Error)
                .Using(new Error("Received unknown state for event: " + state.FsmEvent)));

            When(State.Success, ev =>
            {
                var success = ev.StateData as Success;
                Console.WriteLine($"Success: {success?.Message}");
                return Stop();
            });

            When(State.Error, ev =>
            {
                var error = ev.StateData as Error;
                Console.WriteLine($"Failed: {error?.Message}");
                return Stop();
            });
        }
    }
}
