using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Gradebook;

namespace SmartClass.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Teacher")]
    public sealed class GradebookController : ControllerBase
    {
        private readonly IGradebookService _gradebook;

        public GradebookController(IGradebookService gradebook)
        {
            _gradebook = gradebook;
        }

        /// <summary>Журнал оцінок по класу</summary>
        [HttpGet("classroom/{classroomId:guid}")]
        public async Task<ActionResult<GradebookDto>> GetForClassroom(
            [FromRoute] Guid classroomId,
            CancellationToken ct)
        {
            var dto = await _gradebook.GetForClassroomAsync(classroomId, ct);
            return Ok(dto);
        }
    }
}
