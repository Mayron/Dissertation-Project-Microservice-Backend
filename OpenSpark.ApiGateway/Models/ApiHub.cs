using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OpenSpark.ActorModel.Commands;
using OpenSpark.Domain;

namespace OpenSpark.ApiGateway.Models
{
    public class ApiHub : Hub
    {
        [Authorize]
        public void ProtectedEndpoint()
        {
            //            DiscussionsActorSystem.ActorReferences.SignalRBridge.Tell(new ConnectUserCommand(user), null);

            Console.WriteLine("User Authorized");
            var context = Context;
            var user = context.User;
            var claims = user.Claims;
            var identity = user.Identity;
            var isAuthentication = identity.IsAuthenticated;

            // to response to the user/client:
//            Clients.Caller.SendAsync("SomeClientMethod", isAuthentication);

            Console.WriteLine(isAuthentication);
        }

        public void DisconnectUser(string id)
        {
            Console.WriteLine("User disconnected");
            var context = Context;
            var user = context.User;
            var identity = user.Identity;
            var isAuthentication = identity.IsAuthenticated;

            Console.WriteLine(isAuthentication);
        }

        public override Task OnConnectedAsync()
        {
            // a unique connection id for that user
            Console.WriteLine("User connected");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("User disconnected");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
