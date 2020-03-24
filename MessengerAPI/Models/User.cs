using System.Collections.Generic;

namespace WebApplication.Models
{
    public class User
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public virtual List<User> Dialogues { get; set; }
        public string PublicKey { get; set; }
    }
    
    public class UserLogin
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class UserSignup
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }
}