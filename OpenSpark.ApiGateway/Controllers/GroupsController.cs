using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenSpark.ApiGateway.Handlers.Commands;
using OpenSpark.ApiGateway.InputModels;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Controllers
{
    [Route("api/groups")]
    public class GroupsController : BaseController
    {
        public GroupsController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost("create")]
        public async Task<ActionResult<string>> Create(NewGroupInputModel model) =>
            await HandleRequest(new CreateGroup.Command(model));
    }
}