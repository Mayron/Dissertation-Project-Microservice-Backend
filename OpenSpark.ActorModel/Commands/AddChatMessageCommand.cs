using OpenSpark.Domain;

namespace OpenSpark.ActorModel.Commands
{
    public class AddChatMessageCommand
    {
        public User User { get; }
        public ChatMessage ChatMessage { get; }

        public AddChatMessageCommand(User user, ChatMessage message)
        {
            User = user;
            ChatMessage = message;
        }
    }
}
