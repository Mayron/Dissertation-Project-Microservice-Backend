using OpenSpark.Domain;

namespace OpenSpark.Shared.ViewModels
{
    public class UserGroupsViewModel : INamedEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Visibility { get; set; }
    }
}