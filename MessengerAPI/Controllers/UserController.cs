using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private MessengerContext _context;
        private IConfiguration _configuration;

        public UserController(MessengerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("signup")]
        public IActionResult Signup([FromBody] UserSignup signupForm)
        {
            if (_context.Users.Any(u => u.UserName == signupForm.UserName))
                return BadRequest("Username is busy");
            string hashedPassword = GetHash(signupForm.Password);
            _context.Users.Add(new User
            {
                UserName = signupForm.UserName, 
                Name = signupForm.Name,
                Password = hashedPassword,
            });
            _context.SaveChanges();
            
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLogin loginForm)
        {
            IActionResult response = Unauthorized();
            User user = AuthenticateUser(loginForm);
            if (user != null)
            {
                string tokenString = GenerateToken(user);
                response = Ok(new {token = tokenString});
            }

            return response;
        }

        [Authorize]
        [HttpGet("CreateDialogue")]
        public IActionResult CreateDialogue(string userName)
        {
            var userOne = GetCurrentUser(HttpContext.User);
            var userTwo = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (userTwo == null || userOne == null) 
                return NotFound();

            if (userOne.Dialogues == null)
                userOne.Dialogues = new List<User>();
            if (userTwo.Dialogues == null)
                userTwo.Dialogues = new List<User>();
            
            userOne.Dialogues.Add(userTwo);
            userTwo.Dialogues.Add(userOne);
            
            _context.SaveChanges();

            return Ok();
        }
        
        
        [NonAction]
        public string GenerateToken(User user) // another class for claims
        {
            SymmetricSecurityKey securityKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("userName", user.UserName),
            };

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddHours(12),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        [NonAction]
        public User AuthenticateUser(UserLogin loginForm)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == loginForm.UserName);
            if (user != null && user.Password == GetHash(loginForm.Password)) // Compare hashes later
                return user;
            return null;
        }
        
        [NonAction]
        public User GetCurrentUser(ClaimsPrincipal userClaims)
        {
            if (!userClaims.HasClaim(c => c.Type == "id"))
                return null;
            var userId =
                Int64.Parse(userClaims.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            return _context.Users.Find(userId);
        }

        [NonAction]
        private string GetHash(string str)
        {
            using (var sha = new SHA256CryptoServiceProvider())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(str));
                return Encoding.UTF8.GetString(hash);
            }
        }
    }
}