using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands
{
    public class AddChatMessageCommand : ICommand
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
