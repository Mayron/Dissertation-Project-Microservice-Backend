using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Sagas
{
    public class ExecuteCreatePostSagaCommand : ISagaExecutionCommand
    {
        public User User { get; set; }
        public string GroupId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public MetaData MetaData { get; set; }
    }
}