public  class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}