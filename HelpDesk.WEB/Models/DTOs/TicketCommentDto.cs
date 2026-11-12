namespace HelpDesk.WEB.Models.DTOs
{
    public sealed record TicketCommentDto
    {
        public string AuthorName { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
        public bool IsInternal { get; init; }
        public DateTime CreatedAtUtc { get; init; }
    }
}
