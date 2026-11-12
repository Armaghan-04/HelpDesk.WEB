namespace HelpDesk.WEB.Models.DTOs
{
    public sealed class AddCommentRequest
    {
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Internal comments are only visible to technicians and managers.
        /// The service is responsible for only allowing this for staff.
        /// </summary>
        public bool IsInternal { get; set; }
    }
}
