using System.Text.RegularExpressions;

namespace Shard.EnzoSamy.Api.Utilities
{
    public static class ValidationUtils
    {
        private static readonly string UserIdPattern = "^[a-zA-Z0-9_-]+$";

        public static bool IsValidUserId(string userId)
        {
            return Regex.IsMatch(userId, UserIdPattern);
        }
    }
}