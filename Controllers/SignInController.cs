using Microsoft.AspNetCore.Mvc;
using activityCore.Data;
using activityCore.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class SignInController : ControllerBase

{
    private const string TokenSecret = "YourSecretKeyForAuthenticationOfApplicationDeveloper";

    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(1);
    private ActivityContext _db = new ActivityContext();

    private readonly ILogger<SignInController> _logger;

    public SignInController(ILogger<SignInController> logger)
    {
        _logger = logger;
    }

    public struct SignInCreate
    {
        /// <summary>
        /// Username of User
        /// </summary>
        /// <example>John</example>
        /// <required>true</required>
        public string? Username { get; set; }

        /// <summary>
        /// Password of User
        /// </summary>
        /// <example>1234</example>
        /// <required>true</required>
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
    [HttpPost("token", Name = "GenerateToken")]
    public string GenerateToken([FromBody] Account user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
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
    /// ```json
    /// {
    ///     "id": 2,
    ///     "username": "admin",
    ///     "password": "1234",
    ///     "role": "admin",
    ///     "createDate": "2024-05-08T14:57:20.942Z",
    ///     "updateDate": "2024-05-08T14:57:20.942Z",
    ///     "isDelete": false
    /// }
    /// ```
    /// </remarks>
    /// <returns></returns>
    /// <response code="200">
    /// Success
    ///  ```json
    /// {
    ///     "Code": 200,
    ///     "Message": "Success",
    ///     "Data": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFkbWluIiwibmJmIjoxNzE1MTgwNzM0LCJleHAiOjE3MTUxODA3OTQsImlhdCI6MTcxNTE4MDczNCwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo4MDAwIiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo4MDAwIn0.fJlV5TqX63mY0g9xHT48PH8W1rgARNocPbpaNjA8myQ"
    /// }
    /// ```
    /// </response>
    /// <response code="401">
    /// Unauthorized
    ///  ```json
    /// {
    ///     "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2",
    ///     "title": "Unauthorized",
    ///     "status": 401,
    ///     "traceId": "00-7856af05e8d849842fb14999ce754c4c-6e3b0369d3eb3642-00"
    /// }
    /// ```
    /// </response>
    [HttpPost(Name = "SignIn")]
    public IActionResult SignIn([FromBody] Account requestUser)
    {
        Account? user = _db.Accounts.FirstOrDefault(doc => doc.Username == requestUser.Username && doc.Password == requestUser.Password && doc.IsDelete == false);
        string bearerToken = "";
        if (user == null)
        {
            return Unauthorized();
        }
        try
        {
            bearerToken = GenerateToken(user);
        }
        catch
        {
            return StatusCode(500);
        }
        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = bearerToken
        });
    }




}