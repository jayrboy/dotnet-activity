using Microsoft.AspNetCore.Mvc;
using activityCore.Data;
using activityCore.Models;
using Microsoft.AspNetCore.Identity;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private ActivityContext _db = new ActivityContext();

    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }

    public struct UserCreate
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

    [HttpPost(Name = "CreateUser")]
    public ActionResult CreateUser(UserCreate userCreate)
    {
        User user = new User
        {
            Username = userCreate.Username,
            Password = userCreate.Password,
            Token = userCreate.Token,
        };

        user = activityCore.Models.User.Create(_db, user);

        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = user
        });
    }



}