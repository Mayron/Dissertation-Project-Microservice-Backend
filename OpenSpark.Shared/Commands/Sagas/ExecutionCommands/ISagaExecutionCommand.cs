namespace OpenSpark.Shared.Commands.Sagas.ExecutionCommands
{
    public interface ISagaExecutionCommand : ISagaCommand
    {
        string SagaName { get; set; }
    }
}