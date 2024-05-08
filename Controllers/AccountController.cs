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
public class AccountController : ControllerBase

{
    private const string TokenSecret = "YourSecretKeyForAuthenticationOfApplicationDeveloper";

    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(1);
    private ActivityContext _db = new ActivityContext();

    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }

    public struct AccountCreate
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
        /// Token of JWT
        /// </summary>
        /// <example>YourSecretKeyForAuthenticationOfApplication</example>
        /// <required>true</required>
        public string? Token { get; set; }
    }

    /// <summary>
    /// Create User Account
    /// </summary>
    [HttpPost(Name = "CreateUser")]
    public ActionResult CreateUser(AccountCreate AccountCreate)
    {
        Account user = new Account
        {
            Username = AccountCreate.Username,
            Password = AccountCreate.Password,
            Token = AccountCreate.Token,
        };

        user = Account.Create(_db, user);

        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = user
        });
    }

    /// <summary>
    /// Get All
    /// </summary>
    [HttpGet(Name = "GetAllUsers")]
    public ActionResult GetAllUsers()
    {
        List<Account> users = Account.GetAll(_db).ToList();
        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = users
        });
    }

    /// <summary>
    /// Get ID
    /// </summary>
    [HttpGet("{id}", Name = "GetUserById")]
    public ActionResult GetUserById(int id)
    {
        Account user = Account.GetById(_db, id);
        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = user
        });
    }

    /// <summary>
    /// Update
    /// </summary>
    [HttpPut(Name = "UpdateUser")]
    public ActionResult UpdateUser(Account user)
    {
        user = Account.Update(_db, user);
        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = user
        });
    }

    /// <summary>
    /// Delete ID
    /// </summary>
    [HttpDelete("{id}", Name = "DeleteUserById")]
    public ActionResult DeleteUserById(int id)
    {
        Account user = Account.Delete(_db, id);
        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = user
        });
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
    [HttpPost("login")]
    public IActionResult Login([FromBody] Account requestUser)
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