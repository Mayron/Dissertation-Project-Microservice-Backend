namespace OpenSpark.Shared.Commands.SagaExecutionCommands
{
    public interface ISagaExecutionCommand : ICommand
    {
        string SagaName { get; set; }
    }
}