namespace OpenSpark.Shared.Domain
{
    public interface INamedEntity : IEntity
    {
        string Name { get; set; }
    }
}