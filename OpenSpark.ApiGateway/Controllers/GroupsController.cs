using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenSpark.ApiGateway.Handlers;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.Shared.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Controllers
{
    [ApiController]
    [Authorize]
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
        [HttpPost("create")]
        public async Task<ActionResult<ValidationResult>> Create(NewGroupInputModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));

            var result = await _mediator.Send(new CreateGroup.Command(model));

            if (result.IsValid)
                return Accepted(result);

            return BadRequest(result);
        }
    }
}