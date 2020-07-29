using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenSpark.ApiGateway.Handlers;
using OpenSpark.ApiGateway.InputModels;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Handlers.Commands;

namespace OpenSpark.ApiGateway.Controllers
{
    [Route("api/posts")]
    public class PostsController : BaseController
    {
        public PostsController(IMediator mediator) : base(mediator) {}

        [HttpPost]
        public async Task<ActionResult<string>> Post(NewPostInputModel model) =>
            await HandleRequest(new CreatePost.Command(model));

        [HttpPost("comment")]
        public async Task<ActionResult<string>> Comment(CommentInputModel model) =>
            await HandleRequest(new CreateComment.Command(model));

        [HttpPost("comment/vote")]
        public async Task<ActionResult<string>> Vote(ChangeVoteInputModel model) =>
            await HandleRequest(new ChangeVote.Command(model));
    }
}