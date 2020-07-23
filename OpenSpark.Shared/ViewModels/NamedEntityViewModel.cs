using OpenSpark.Domain;

namespace OpenSpark.Shared.ViewModels
{
    public class NamedEntityViewModel : INamedEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}