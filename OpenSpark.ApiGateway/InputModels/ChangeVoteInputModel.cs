using System.ComponentModel.DataAnnotations;

namespace OpenSpark.ApiGateway.InputModels
{
    public class ChangeVoteInputModel
    {
        [Required(ErrorMessage = "Missing post id")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Invalid post id")]
        public string PostId { get; set; }

        // Can be null (not required)
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Invalid post id")]
        public string CommentId { get; set; }

        [Range(-1, 1, ErrorMessage = "Invalid vote value")]
        public int Vote { get; set; }
    }
}