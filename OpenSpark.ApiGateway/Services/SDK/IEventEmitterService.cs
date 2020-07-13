using OpenSpark.Shared.Payloads;

namespace OpenSpark.ApiGateway.Services.SDK
{
    public interface IEventEmitterService
    {
        void ReceivedPayload(NewsFeedPostsPayload payload);
    }
}
