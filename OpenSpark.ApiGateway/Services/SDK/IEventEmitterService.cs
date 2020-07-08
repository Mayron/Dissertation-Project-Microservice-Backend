using OpenSpark.Discussions.Payloads;

namespace OpenSpark.ApiGateway.Services.SDK
{
    public interface IEventEmitterService
    {
        void ReceivedPayload(NewsFeedPostsPayload payload);
    }
}
