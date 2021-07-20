namespace ShopRestAPI.Models
{
    public class User
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }

    public class UserDTO
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
