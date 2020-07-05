using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using OpenSpark.ApiGateway.Services.SDK;
using OpenSpark.ApiGateway.ViewModels;

namespace OpenSpark.ApiGateway.Services
{
    public class NewsFeedService : INewsFeedService
    {
        public Task<List<PostViewModel>> GetPostsAsync()
        {
            var posts = new List<PostViewModel>
            {
                new PostViewModel
                {
                    Author = "James",
                    Body = "Hello, this is a nice place you have here!",
                    Header = "Hi, I'm new here",
                    Url = "/u/james/post/this-is-the-title-of-the-post",
                    When = "16 hours ago"
                },
                new PostViewModel
                {
                    Author = "Mayron",
                    Header = "Player X has been banned from YouTube - discussions thread!",
                    Url = "/u/mayron/post/player-x-has-been-banned-from-youtube",
                    When = "1 day ago"
                },
                new PostViewModel
                {
                    Author = "Mike",
                    Body = HttpUtility.HtmlEncode("# Lorem ipsum dolor sit amet,\n\n\n## consectetur adipiscing elit\n\n\n, <p style='background-color: red;'>sed do eiusmod<p> tempor incididunt **ut labore et dolore** magna aliqua.*Mauris cursus mattis molestie a iaculis at.* Cras tincidunt lobortis feugiat vivamus. Urna et pharetra pharetra massa massa ultricies mi.\n### Aenean et tortor\n at risus viverra adipiscing at in tellus."),
                    Url = "/u/mayron/post/player-x-has-been-banned-from-youtube",
                    When = "1 day ago"
                },
            };

            return Task.FromResult(posts);
        }
    }
}
