namespace Shard.EnzoSamy.Api.Security
{
    public class AdminCredentials
    {
        public string Username { get; }
        public string Password { get; }

        public AdminCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}