using System;
using Akka.Actor;
using OpenSpark.ActorModel.Actors;
using OpenSpark.ActorModel.Services;

namespace OpenSpark.ApiGateway.Models
{
    // A singleton:
    public static class DiscussionsActorSystem
    {
        private static ActorSystem _actorSystem;
        private static IEventEmitter _eventEmitter;

        public static void Create(IEventEmitter eventEmitter)
        {
            _eventEmitter = eventEmitter;
            _actorSystem = ActorSystem.Create("DiscussionsSystem");

//            ActorReferences.UserManager = _actorSystem.ActorOf<UserManagerActor>();

            // Using Remote actor
//            ActorReferences.UserManager = _actorSystem
//                .ActorSelection("akka.tcp://DiscussionsSystem@127.0.0.1:8091/user/UserManager")
//                .ResolveOne(TimeSpan.FromSeconds(3)).Result;

            // A local actor
//            ActorReferences.SignalRBridge = _actorSystem.ActorOf(
//                Props.Create(() => new SignalRBridgeActor(_eventEmitter, ActorReferences.UserManager)),
//                "SignalRBridge");

            // If there are no props needed (no constructor args) use:
//            ActorReferences.UserManager = _actorSystem.ActorOf(Props.Create<UserManagerActor>());
        }

        public static async void TerminateAsync()
        {
            await _actorSystem.Terminate();
        }

        public static class ActorReferences
        {
            public static IActorRef UserManager { get; set; }
            public static IActorRef SignalRBridge { get; set; }
        }
    }
}
