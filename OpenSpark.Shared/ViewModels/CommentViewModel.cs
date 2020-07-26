namespace OpenSpark.Shared.ViewModels
{
    public class CommentViewModel
    {
        public string When { get; set; }
        public string CommentId { get; set; }
        public string PostId { get; set; }
        public string Body { get; set; }
        public string AuthorUserId { get; set; }
        public int Votes { get; set; }
        public string AuthorDisplayName { get; set; }
    }
}