namespace OpenSpark.Shared.ViewModels
{
    public class PostViewModel
    {
        public string AuthorUserId { get; set; }
        public int TotalComments { get; set; }
        public string PostId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string When { get; set; }
        public int Votes { get; set; }
        public string GroupId { get; set; }
        public string AuthorDisplayName { get; set; }
        public string GroupName { get; set; }
    }
}
