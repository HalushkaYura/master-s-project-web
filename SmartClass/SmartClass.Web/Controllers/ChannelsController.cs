using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer; // 🔹 додай це
using SmartClass.Application.Abstractions;
using SmartClass.Domain.Entities;
using SmartClass.Domain.Enums;
using SmartClass.Infrastructure.Persistence;
using SmartClass.Web.Hubs;

namespace SmartClass.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // 🔹 ЯВНО кажемо: JWT
    public sealed class ChannelsController : ControllerBase
    {
        private readonly IRepository<Channel> channelsRepo;
        private readonly IDbContextFactory<AppDbContext> dbFactory;

        public ChannelsController(
            IRepository<Channel> channelsRepo,
            IDbContextFactory<AppDbContext> dbFactory)
        {
            this.channelsRepo = channelsRepo;
            this.dbFactory = dbFactory;
        }

        [HttpGet("by-classroom/{classroomId:guid}")]
        public async Task<ActionResult<ChannelDto>> GetChannelForClassroom(
            Guid classroomId,
            CancellationToken ct)
        {
            var channel = await channelsRepo.GetEntityAsync(
                c => c.ClassroomId == classroomId && c.Type == ChannelType.Class);

            if (channel == null)
                return NotFound("Channel not found for this classroom.");

            return Ok(new ChannelDto(
                channel.Id,
                channel.ClassroomId,
                channel.Title
            ));
        }

        [HttpGet("{channelId:guid}/messages")]
        public async Task<ActionResult<IReadOnlyList<ChatMessageDto>>> GetMessages(
            Guid channelId,
            int skip = 0,
            int take = 50,
            CancellationToken ct = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);

            var query =
                from m in db.Messages
                join u in db.Users on m.AuthorId equals u.Id into userJoin
                from u in userJoin.DefaultIfEmpty()
                where m.ChannelId == channelId
                orderby m.CreatedAt descending
                select new ChatMessageDto(
                    m.Id,
                    m.ChannelId,
                    m.AuthorId,
                    u.UserName ?? "Користувач",
                    m.Text,
                    m.CreatedAt
                );

            var list = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);

            list.Reverse();

            return Ok(list);
        }
    }
}
