namespace HelpDesk.WEB.Common.Results
{
    /// <summary>
    /// Immutable description of an expected application failure.
    /// </summary>
    public sealed record Error(string Code, string Description)
    {
        /// <summary>
        /// Represents the absence of an error on a successful result.
        /// </summary>
        public static readonly Error None = new(string.Empty, string.Empty);
    }
}
