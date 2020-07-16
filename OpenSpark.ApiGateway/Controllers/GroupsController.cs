using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenSpark.ApiGateway.Handlers;
using OpenSpark.ApiGateway.InputModels;
using System.Linq;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Models;

namespace OpenSpark.ApiGateway.Controllers
{
    [ApiController]
    [Route("api/groups")]
    public class GroupsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GroupsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// POST /api/groups/create
        /// </summary>
        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult<ValidationResult>> Create(NewGroupInputModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));

            var result = await _mediator.Send(new CreateGroup.Command(model, User));

            if (result.IsValid)
                return Accepted(result);

            return BadRequest(result);
        }
    }
}