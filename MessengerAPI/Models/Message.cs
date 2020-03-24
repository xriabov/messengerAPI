using System;

namespace WebApplication.Models
{
    public class Message
    {
        public ulong Id { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public User Sender { get; set; }
        public User Recipient { get; set; }
        public bool IsRead { get; set; }
    }

    public class MessagePost
    {
        public string Content { get; set; }
    }

    public class MessageGet
    {
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public string SenderName { get; set; }
        public string RecipientName { get; set; }
        public bool IsRead { get; set; }
    }
}