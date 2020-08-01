using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenSpark.ApiGateway.Handlers.Commands;
using OpenSpark.ApiGateway.InputModels;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Controllers
{
    [Route("api/teams")]
    public class TeamsController : BaseController
    {
        public TeamsController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost("permission")]
        public async Task<ActionResult<string>> Permission(ChangePermissionInputModel model) =>
            await HandleRequest(new ChangePermission.Command(model));

        [HttpPost("create")]
        public async Task<ActionResult<string>> Create(NewTeamInputModel model) =>
            await HandleRequest(new CreateTeam.Command(model));
    }
}