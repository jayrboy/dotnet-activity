using Microsoft.AspNetCore.Mvc;
using activityCore.Data;
using activityCore.Models;
using Microsoft.AspNetCore.Authorization;

namespace activityCore.Controllers
{
    [Authorize(Roles = "admin, dev")]
    [ApiController]
    [Route("api/[controller]")]
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
            /// Role of User
            /// </summary>
            /// <example>user</example>
            /// <required>true</required>
            public string? Role { get; set; }
        }

        /// <summary>
        /// Create User User
        /// </summary>
        [HttpPost(Name = "CreateUser")]
        public ActionResult CreateUser(UserCreate UserCreate)
        {
            User user = new User
            {
                Username = UserCreate.Username,
                Password = UserCreate.Password,
                Role = UserCreate.Role,
            };
            try
            {
                user = Models.User.Create(_db, user);
            }
            catch
            {
                // Handle unique constraint violation (duplicate username)
                return StatusCode(409, new Response
                {
                    Code = 409,
                    Message = "Username already exists",
                    Data = null
                });
            }
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
            List<User> users = Models.User.GetAll(_db).ToList();
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
            User user = Models.User.GetById(_db, id);
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
        public ActionResult UpdateUser(User user)
        {
            user = Models.User.Update(_db, user);
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
            try
            {
                User user = Models.User.Delete(_db, id);
                return Ok(new Response
                {
                    Code = 200,
                    Message = "Success",
                    Data = user
                });
            }
            catch
            {
                // ถ้าไม่พบข้อมูล user ตาม id ที่ระบุ
                return NotFound(new Response
                {
                    Code = 404,
                    Message = "User not found",
                    Data = null
                });
            }


        }
    }
}