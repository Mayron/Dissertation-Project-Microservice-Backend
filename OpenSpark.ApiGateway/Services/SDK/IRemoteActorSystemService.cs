using OpenSpark.Shared.Commands;

namespace OpenSpark.ApiGateway.Services.SDK
{
    public interface IRemoteActorSystemService
    {
        void Send(ICommand command);
    }
}