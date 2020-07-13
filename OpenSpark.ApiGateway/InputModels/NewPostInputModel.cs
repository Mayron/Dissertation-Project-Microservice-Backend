namespace OpenSpark.ApiGateway.InputModels
{
    public class NewPostInputModel
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string GroupId { get; set; }

        // The user id of the author
        public string AuthorUserId { get; set; }
    }
}
