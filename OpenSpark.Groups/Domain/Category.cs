using OpenSpark.Shared.Domain;

namespace OpenSpark.Groups.Domain
{
    public class Category : INamedEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}