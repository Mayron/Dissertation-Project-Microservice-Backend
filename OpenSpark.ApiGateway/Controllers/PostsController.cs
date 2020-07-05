using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenSpark.ApiGateway.Handlers;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.ViewModels;

namespace OpenSpark.ApiGateway.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PostsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET api/posts
        [HttpGet]
        public async Task<ActionResult<List<PostViewModel>>> Get()
        {
            return await _mediator.Send(new NewsFeedPosts.Query());
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Post(NewPostInputModel model)
        {
            Console.WriteLine(JsonConvert.SerializeObject(model));

            return Ok("fine");
        }
    }
}
