using Microsoft.AspNetCore.Mvc;
using activityCore.Data;
using activityCore.Models;
using Microsoft.AspNetCore.Authorization;

[Authorize]
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AccountController : ControllerBase

{
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
        /// Role of User
        /// </summary>
        /// <example>user</example>
        /// <required>true</required>
        public string? Role { get; set; }
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
            Role = AccountCreate.Role,
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
    /// <remarks>
    /// Sample request:
    /// ```json
    /// {
    ///     "id": 1,
    ///     "username": "ChangeU",
    ///     "password": "ChangeP",
    ///     "createDate": "2024-05-08T14:57:20.942Z",
    ///     "updateDate": "2024-05-08T14:57:20.942Z",
    ///     "isDelete": false
    /// }
    /// ```
    /// </remarks>
    /// <returns></returns>
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
}