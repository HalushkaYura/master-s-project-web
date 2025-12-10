public record ChatMessageDto(
    Guid Id,
    Guid ChannelId,
    Guid AuthorId,
    string AuthorName,
    string Text,
    DateTime CreatedAt
);