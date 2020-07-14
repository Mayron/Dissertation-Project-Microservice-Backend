using Akka.Actor;

namespace OpenSpark.ApiGateway.Models.StateData
{
    public interface IStateData {}

    public class Uninitialized : IStateData
    {
        public static Uninitialized Instance = new Uninitialized();
    }

    public class Error : IStateData
    {
        public string Message { get; }

        public Error(string message)
        {
            Message = message;
        }
    }

    public class Success : IStateData
    {
        public string Message { get; }

        public Success(string message)
        {
            Message = message;
        }
    }

    public class VerifyingUserData : IStateData
    {
        public IActorRef Target { get; }
    }

    public class AddingPostData : IStateData
    {

    }
}
