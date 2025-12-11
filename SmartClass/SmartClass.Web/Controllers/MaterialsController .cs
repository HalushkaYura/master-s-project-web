using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClass.Application.Abstractions;

namespace SmartClass.Web.Controllers
{
    [ApiController]
    [Route("api/materials")]
    [Authorize(Roles = "Teacher")]
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialService materialService;

        public MaterialsController(IMaterialService materialService)
        {
            this.materialService = materialService;
        }

        [HttpPost("{materialId:guid}/files")]
        [RequestSizeLimit(100_000_000)] // 100 MB
        public async Task<IActionResult> UploadFile(Guid materialId, [FromForm] IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не передано.");

            // classroomId можна або передати як параметр, або взяти з DTO/БД
            // для простоти – передається як query ?classroomId=
            if (!Guid.TryParse(HttpContext.Request.Query["classroomId"], out var classroomId))
                return BadRequest("classroomId required");

            var ownerId = Guid.Parse(User.FindFirst("sub")!.Value);

            await using var stream = file.OpenReadStream();

            var dto = await materialService.UploadFileAsync(
                classroomId,
                materialId,
                ownerId,
                file.FileName,
                file.ContentType ?? "application/octet-stream",
                file.Length,
                stream,
                ct);

            return Ok(dto);
        }
    }

}
