using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Sagas
{
    public class ExecuteConnectProjectSagaCommand : ISagaExecutionCommand
    {
        public User User { get; set; }
        public string ProjectId { get; set; }
        public string GroupId { get; set; }
        public MetaData MetaData { get; set; }
    }
}