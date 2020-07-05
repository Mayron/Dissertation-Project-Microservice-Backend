using OpenSpark.Domain;

namespace OpenSpark.ActorModel.Services
{
    public interface IEventEmitter
    {
        void ChatMessageSent(User user, ChatMessage message);
        void UserConnected(User user);
    }
}
