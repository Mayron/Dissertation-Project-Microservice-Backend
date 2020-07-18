namespace OpenSpark.Domain
{
    public interface INamedEntity : IEntity
    {
        string Name { get; set; }
    }
}