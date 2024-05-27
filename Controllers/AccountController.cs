using Microsoft.AspNetCore.Mvc;
using activityCore.Data;
using activityCore.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace activityCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private const string TokenSecret = "YourSecretKeyForAuthenticationOfApplicationDeveloper";

        private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(10);

        private ActivityContext _db = new ActivityContext();

        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        public struct RegisterCreate
        {
            /// <summary>
            /// Username of User
            /// </summary>
            /// <example>John</example>
            /// <required>true</required>
            [Required(ErrorMessage = "User Name is required")]
            [RegularExpression(@"^[a-zA-Z0-9]*$")]
            public string? Username { get; set; }

            /// <summary>
            /// Password of User
            /// </summary>
            /// <example>1234</example>
            /// <required>true</required>
            [Required(ErrorMessage = "Password is required")]
            public string? Password { get; set; }

            /// <summary>
            /// Role of User
            /// </summary>
            /// <example>user</example>
            /// <required>true</required>
            public string? Role { get; set; }
        }

        /// <summary>
        /// Generate Token
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /GenerateToken
        ///     {
        ///         "id": 0,
        ///         "username": "string",
        ///         "password": "string",
        ///         "role": "admin",
        ///         "createDate": "2024-05-27T15:02:54.076Z",
        ///         "updateDate": "2024-05-27T15:02:54.076Z",
        ///         "isDelete": true
        ///     }
        ///     
        /// </remarks>
        [HttpPost("token", Name = "GenerateToken")]
        public string GenerateToken([FromBody] User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(TokenSecret);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role) //TODO: Add Role parameter
            // Add more claims as needed
        };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TokenLifetime),
                Issuer = "http://localhost:8000",
                Audience = "http://localhost:8000",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            if (token != null)
            {
                var jwt = tokenHandler.WriteToken(token);
                return jwt;
            }
            else
            {
                return "Failed to write token.";
            }
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /Login
        ///     {
        ///         "username": "admin",
        ///         "password": "1234"
        ///     }
        ///     
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">
        /// Success
        /// 
        ///     {
        ///         "Code": 200,
        ///         "Message": "Success",
        ///         "Data": "token"
        ///     }
        /// 
        /// </response>
        /// <response code="409">
        /// Conflict
        ///     
        ///     {
        ///         "code": 409,
        ///         "message": "Username already exists",
        ///         "data": null
        ///     }
        ///     
        /// </response>
        [HttpPost("Login", Name = "Login")]
        public IActionResult Login([FromBody] User request)
        {
            User? user = _db.Users.FirstOrDefault(doc => doc.Username == request.Username && doc.Password == request.Password && doc.IsDelete == false);
            string bearerToken;

            try
            {
                bearerToken = GenerateToken(user);
            }
            catch
            {
                return BadRequest(new Response
                {
                    Code = 400,
                    Message = "Bad Request to Username & Password",
                    Data = null
                }
                );
            }
            return Ok(new Response
            {
                Code = 200,
                Message = "Login Success",
                Data = new
                {
                    token = bearerToken
                }
            });
        }

        /// <summary>
        /// Register
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /Register
        ///     {
        ///         "username": "John",
        ///         "password": "1234"
        ///     }
        ///     
        /// </remarks>
        /// <returns></returns>
        /// <response code="201">
        /// Created Successfully
        /// 
        ///     {
        ///         "Code": 201,
        ///         "Message": "Created Successfully",
        ///         "Data": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFkbWluIiwibmJmIjoxNzE1MTgwNzM0LCJleHAiOjE3MTUxODA3OTQsImlhdCI6MTcxNTE4MDczNCwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo4MDAwIiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo4MDAwIn0.fJlV5TqX63mY0g9xHT48PH8W1rgARNocPbpaNjA8myQ"
        ///     }
        ///     
        /// </response>
        /// <response code="409">
        /// Conflict
        /// 
        ///     {
        ///         "code": 409,
        ///         "message": "Username already exists",
        ///         "data": null
        ///     }
        ///     
        /// </response>
        [HttpPost("Register", Name = "Register")]
        public ActionResult Register(RegisterCreate registerCreate)
        {
            if (registerCreate.Role.IsNullOrEmpty())
            {
                registerCreate.Role = "user";
            }

            User user = new User
            {
                Username = registerCreate.Username,
                Password = registerCreate.Password,
                Role = registerCreate.Role,
            };

            try
            {
                user = Models.User.Create(_db, user);
            }
            catch
            {
                // Handle unique key constraint violation (duplicate username)
                return StatusCode(409, new Response
                {
                    Code = 409,
                    Message = "Username already exists",
                    Data = null
                });
            }

            return user.Username == null || user.Password == null || user.Role == null
                ? BadRequest(new Response
                {
                    Code = 400,
                    Message = "Bad Request",
                    Data = null
                })
                : Ok(new Response
                {
                    Code = 201,
                    Message = "Created Successfully",
                    Data = user
                });
        }
    }
}