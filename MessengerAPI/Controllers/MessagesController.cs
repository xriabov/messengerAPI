using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private IConfiguration _configuration;
        private MessengerContext _context;

        public MessagesController(MessengerContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpGet("{id}")]
        [Authorize]
        public ActionResult<List<MessageGet>> GetMessages(long id)
        {
            User current = new UserController(_context, _configuration).GetCurrentUser(HttpContext.User);
            User second = _context.Users.Find(id);

            if (second == null)
                return NotFound();
            if (current.Dialogues == null || second.Dialogues == null)
                return BadRequest();
            if (!(current.Dialogues.Contains(second) && second.Dialogues.Contains(current)))
                return BadRequest();

            List<MessageGet> messages = _context.Messages
                .Where(m =>
                    (m.Sender == current && m.Recipient == second) ||
                    (m.Sender == second && m.Recipient == current))
                .OrderBy(m => m.Date)
                .Select(m => new MessageGet
                {
                    Content = m.Content,
                    Date = m.Date,
                    IsRead = m.IsRead,
                    RecipientName = m.Recipient.UserName,
                    SenderName = m.Sender.UserName
                })
                .ToList();
            return messages;
        }

        [HttpPost("{id}")]
        [Authorize]
        public IActionResult SendMessage(long id, [FromBody] MessagePost message)
        {
            User current = new UserController(_context, _configuration).GetCurrentUser(HttpContext.User);
            User second = _context.Users.Find(id);

            if (second == null)
                return BadRequest("No user with such id");
            if (!(current.Dialogues.Contains(second) && second.Dialogues.Contains(current)))
                return Conflict();

            _context.Messages.Add(new Message
            {
                Content = message.Content,
                Date = DateTime.Now,
                IsRead = false,
                Sender = current,
                Recipient = second,
            });
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet("read/{id}")]
        [Authorize]
        public IActionResult ReadMessages(long id)
        {
            User current = new UserController(_context, _configuration).GetCurrentUser(HttpContext.User);
            User second = _context.Users.Find(id);

            foreach (var msg in _context.Messages.Where(m =>
                m.Sender == second && m.Recipient == current && m.IsRead == false))
                msg.IsRead = true;
            _context.SaveChanges();
            return Ok();
        }
    }
}
