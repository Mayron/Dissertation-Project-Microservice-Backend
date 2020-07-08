using OpenSpark.Discussions.Commands;

namespace OpenSpark.ApiGateway.Services.SDK
{
    public interface IRemoteActorSystemService
    {
        void Send(FetchNewsFeedCommand command);
        void Send(ConnectUserCommand command);
        void Send(DisconnectUserCommand command);
    }
}