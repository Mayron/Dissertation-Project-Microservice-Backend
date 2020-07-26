using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenSpark.ApiGateway.Handlers;
using OpenSpark.ApiGateway.InputModels;
using System.Linq;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/posts")]
    public class PostsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PostsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpPost]
        public async Task<ActionResult<string>> Post(NewPostInputModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));

            var result = await _mediator.Send(new CreatePost.Query(model));

            if (result.IsValid)
                return Accepted(result);

            return BadRequest(result.Message);
        }

        [HttpPost("comment")]
        public async Task<ActionResult<string>> Comment(CommentInputModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));

            var result = await _mediator.Send(new CreateComment.Query(model));

            if (result.IsValid)
                return Accepted(result);

            return BadRequest(result.Message);
        }
    }
}