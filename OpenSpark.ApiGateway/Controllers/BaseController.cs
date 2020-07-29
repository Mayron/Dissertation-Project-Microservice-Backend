using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenSpark.ApiGateway.Filters;
using OpenSpark.Shared.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Controllers
{
    [ApiController]
    [Authorize]
    [FirestoreAuthorize]
    public abstract class BaseController : ControllerBase
    {
        protected readonly IMediator Mediator;

        protected BaseController(IMediator mediator)
        {
            Mediator = mediator;
        }

        protected async Task<ActionResult<string>> HandleRequest(IRequest<ValidationResult> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));

            var result = await Mediator.Send(request);

            if (result.IsValid)
                return Accepted(result);

            return BadRequest(result.Message);
        }
    }
}