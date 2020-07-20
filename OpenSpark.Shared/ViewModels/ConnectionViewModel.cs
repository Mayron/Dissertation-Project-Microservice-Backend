namespace OpenSpark.Shared.ViewModels
{
    public class ConnectionViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Visibility { get; set; }
        public bool Available { get; set; }
        public string NotAvailableMessage { get; set; }
    }
}