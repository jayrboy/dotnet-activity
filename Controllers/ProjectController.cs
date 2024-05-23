using Microsoft.AspNetCore.Mvc;
using activityCore.Data;
using activityCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

// [Authorize(Roles = "admin, user")]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProjectController : ControllerBase
{
    private ActivityContext _db = new ActivityContext();

    private readonly ILogger<ProjectController> _logger;

    public ProjectController(ILogger<ProjectController> logger)
    {
        _logger = logger;
    }

    public struct ProjectCreate
    {
        /// <summary>
        /// Project Name
        /// </summary>
        /// <example>Project123</example>
        /// <required>true</required>
        [Required]
        public string? Name { get; set; }

        /// <summary>
        /// Start Date
        /// </summary>
        /// <example>2022-01-01</example>
        public DateOnly? StartDate { get; set; }

        /// <summary>
        /// Update Date
        /// </summary>
        /// <example>2022-01-31</example>
        public DateOnly? EndDate { get; set; }

        /// <summary>
        /// Activities of Project
        /// </summary>
        /// <example>["string"]</example>
        public List<Activity>? Activities { get; set; }
    }

    /// <summary>
    /// Create Project
    /// </summary>
    /// <param name="project"></param>
    /// <returns>A newly created Project</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /Project
    ///     {
    ///         "name": "Project123",
    ///         "startDate": "2022-01-01",
    ///         "endDate": "2022-01-31",
    ///         "activities": [
    ///             {
    ///                 "name": "Act1",
    ///                 "inverseActivityHeader": [
    ///                     {
    ///                         "name": "Act1.1",
    ///                         "inverseActivityHeader": []
    ///                     }
    ///                 ]
    ///             },
    ///             {
    ///                 "name": "Act2",
    ///                 "inverseActivityHeader": [
    ///                     {
    ///                         "name": "Act2.1",
    ///                         "inverseActivityHeader": []
    ///                     },
    ///                     {
    ///                         "name": "Act2.2",
    ///                         "inverseActivityHeader": [
    ///                             {
    ///                                 "name": "Act2.2.1",
    ///                                 "inverseActivityHeader": []
    ///                             }
    ///                         ]
    ///                     }
    ///                 ]
    ///             }
    ///         ]
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">
    /// Returns the newly created project
    /// 
    ///     {
    ///         "Code": 201,
    ///         "Message": "Returns the newly created project",
    ///         "Data": {
    ///             "id": 1,
    ///             "name": "Project123",
    ///             "startDate": "2022-01-01",
    ///             "endDate": "2022-01-31",
    ///             "createDate": "2024-05-07T11:55:57.0455498+07:00",
    ///             "updateDate": "2024-05-07T11:55:57.0455541+07:00",
    ///             "isDelete": false,
    ///             "activities": []
    ///         }
    ///     }
    /// 
    /// </response>
    /// <response code="400">If the project is null</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost(Name = "CreateProject")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<Response> CreateProject(ProjectCreate projectCreate)
    {
        Project project = new Project
        {
            Name = projectCreate.Name,
            StartDate = projectCreate.StartDate,
            EndDate = projectCreate.EndDate,
        };

        try
        {  // Sub logic
            Activity.SetActivitiesCreate(project, project.Activities, projectCreate.Activities);

            // หลังจากที่สร้างกิจกรรมทั้งหมดแล้ว จึงทำการเพิ่มโปรเจคลงในฐานข้อมูล แล้วส่งกลับไปยัง Response กลับไปยัง Client
            Project.Create(_db, project);
        }
        catch (Exception e)
        {
            return StatusCode(500, new Response
            {
                Code = 500,
                Message = e.Message,
                Data = null
            });
        }
        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = project
        });
    }

    /// <summary>
    /// Get All
    /// </summary>
    /// <returns>All Projects</returns>
    /// <response code="200">Success</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet(Name = "GetAll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult GetAllProduct()
    {
        List<Project> projects = Project.GetAll(_db).OrderBy(q => q.Id).ToList(); // ตามลำดับ

        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = projects
        });
    }

    /// <summary>
    /// Get ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Project By ID</returns>
    /// <response code="200">
    /// Success
    /// 
    ///     {
    ///         "Code": 200,
    ///         "Message": "Success",
    ///         "Data": {
    ///             "id": 1,
    ///             "name": "Project123",
    ///             "startDate": "2022-01-01",
    ///             "endDate": "2022-01-31",
    ///             "createDate": "2024-05-07T11:55:57.0455498+07:00",
    ///             "updateDate": "2024-05-07T11:55:57.0455541+07:00",
    ///             "isDelete": false,
    ///             "activities": []
    ///         }
    ///     }
    ///     
    /// </response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet("{id}", Name = "GetProjectById")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult GetProjectById(int id)
    {
        Project project = Project.GetById(_db, id);

        return project == null
        ? NotFound(new Response
        {
            Code = 404,
            Message = "Not Found",
            Data = project
        })
        : Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = project
        });
    }

    /// <summary>
    /// Update
    /// </summary>
    /// <param name="project"></param>
    /// <returns>A newly updated Project</returns>
    /// <response code="200">Success</response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPut(Name = "UpdateProject")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult UpdateProject(Project project)
    {
        try
        {
            project = Project.Update(_db, project);
        }
        catch (Exception e)
        {
            return StatusCode(500, new Response
            {
                Code = 500,
                Message = "Internal Server Error: " + e.Message,
                Data = null
            });
        }

        return project != null ? Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = project
        }) : StatusCode(404, new Response
        {
            Code = 404,
            Message = "Failed to Project Not Found!",
            Data = null
        });
    }

    /// <summary>
    /// Delete ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Product</returns>
    /// <response code="200">
    /// Success
    ///  ```json
    ///  
    ///     {
    ///         "Code": 200,
    ///         "Message": "Success",
    ///         "Data": {
    ///             "id": 1,
    ///             "name": "Project123",
    ///             "startDate": "2022-01-01",
    ///             "endDate": "2022-01-31",
    ///             "createDate": "2024-05-07T11:55:57.0455498+07:00",
    ///             "updateDate": "2024-05-07T11:55:57.0455541+07:00",
    ///             "isDelete": true,
    ///             "activities": []
    ///         }
    ///     }
    /// 
    /// </response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal Server Error</response>
    [HttpDelete("{id}", Name = "DeleteProjectById")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult DeleteProjectById(int id)
    {
        try
        {
            Project project = Project.Delete(_db, id);

            return Ok(new Response
            {
                Code = 200,
                Message = "Success",
                Data = project
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