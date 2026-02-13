using System.Security.Claims;

namespace TaskManagementAPI.Extention
{
    public static class ClaimsPrincipalExtetions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIDClaim = user.FindFirst(ClaimTypes.NameIdentifier)
                ?? user.FindFirst("sub");
            if(userIDClaim != null && int.TryParse(userIDClaim.Value, out int userId))
            {
                return userId;
            }
            throw new Exception("User ID claim not found or invalid.");
        }
    }
}
