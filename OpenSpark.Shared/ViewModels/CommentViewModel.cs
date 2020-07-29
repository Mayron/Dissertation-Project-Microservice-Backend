namespace OpenSpark.Shared.ViewModels
{
    public class CommentViewModel
    {
        public string CreatedAt { get; set; }
        public string CommentId { get; set; }
        public string PostId { get; set; }
        public string Body { get; set; }
        public string AuthorUserId { get; set; }
        public string AuthorDisplayName { get; set; }
        public int UpVotes { get; set; }
        public int DownVotes { get; set; }
        public bool VotedUp { get; set; }
        public bool VotedDown { get; set; }
    }
}