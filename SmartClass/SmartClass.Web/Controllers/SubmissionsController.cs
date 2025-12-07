using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Abstractions;
using SmartClass.Application.Contracts.Submissions;
using System.Security.Claims;

namespace SmartClass.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class SubmissionsController : ControllerBase
    {
        private readonly ISubmissionService _service;

        public SubmissionsController(ISubmissionService service)
        {
            _service = service;
        }

        private Guid GetUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(sub);
        }

        /// <summary>Студент: гарантувати існування сабміту і повернути його</summary>
        [HttpGet("assignment/{assignmentId:guid}/me")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<SubmissionDetailsDto>> GetMySubmissionForAssignment(
            [FromRoute] Guid assignmentId,
            CancellationToken ct)
        {
            var studentId = GetUserId();

            var dto = await _service.EnsureForStudentAsync(assignmentId, studentId, ct);
            return Ok(dto);
        }

        /// <summary>Викладач: список сабмітів по завданню</summary>
        [HttpGet("assignment/{assignmentId:guid}")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<IReadOnlyList<SubmissionListItemDto>>> GetForAssignment(
            [FromRoute] Guid assignmentId,
            CancellationToken ct)
        {
            var list = await _service.GetForAssignmentAsync(assignmentId, ct);
            return Ok(list);
        }

        /// <summary>Викладач: оцінити сабміт</summary>
        [HttpPost("grade")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Grade([FromBody] GradeSubmissionDto dto, CancellationToken ct)
        {
            var teacherId = GetUserId();
            await _service.GradeAsync(dto, teacherId, ct);
            return Ok();
        }
    }
}
