using OpenSpark.Domain;

namespace OpenSpark.ActorModel.Commands
{
    public class ConnectUserCommand
    {
        public User User { get; set; }

        public ConnectUserCommand(User user)
        {
            User = user;
        }
    }
}
