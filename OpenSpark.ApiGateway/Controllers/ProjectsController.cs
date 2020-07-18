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
    [Route("api/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// POST /api/projects/create
        /// </summary>
        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult<ValidationResult>> Create(NewProjectInputModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));

            var result = await _mediator.Send(new CreateProject.Command(model));

            if (result.IsValid)
                return Accepted(result);

            return BadRequest(result);
        }
    }
}