namespace SmartClass.Web.Hubs
{
    public record ChannelDto(
        Guid Id,
        Guid ClassroomId,
        string Title
    );

}
