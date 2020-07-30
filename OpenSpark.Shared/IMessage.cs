using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared
{
    public interface IMessage
    {
        // Can be null if user is not authenticated
        User User { get; set; }
        MetaData MetaData { get; set; }
    }
}