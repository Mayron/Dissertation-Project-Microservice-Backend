using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenSpark.ApiGateway.Handlers.Commands;
using OpenSpark.ApiGateway.InputModels;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Controllers
{
    [Route("api/projects")]
    public class ProjectsController : BaseController
    {
        public ProjectsController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost("create")]
        public async Task<ActionResult<string>> Create(NewProjectInputModel model) =>
            await HandleRequest(new CreateProject.Command(model));

        [HttpPost("connect")]
        public async Task<ActionResult<string>> Connect(ConnectProjectInputModel model) =>
            await HandleRequest(new ConnectProject.Command(model));
    }
}