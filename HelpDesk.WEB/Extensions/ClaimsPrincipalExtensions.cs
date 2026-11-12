using System.Security.Claims;

namespace HelpDesk.WEB.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the authenticated user's id from the name identifier claim.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the principal has no valid name identifier claim.
        /// </exception>
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            string? value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out Guid userId)
                ? userId
                : throw new InvalidOperationException(
                    "The current user has no valid name identifier claim.");
        }
    }
}
