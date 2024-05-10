using Microsoft.AspNetCore.Mvc;
using activityCore.Data;
using activityCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

// [Authorize]
[ApiController]
[Route("[controller]")]
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

        // public List<Activity>? Activities { get; set; } = new List<Activity>(); // public class ProjectCreate { }

    }

    /// <summary>
    /// Create Project
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// ```json
    /// {
    ///     "name": "Project123",
    ///     "startDate": "2022-01-01",
    ///     "endDate": "2022-01-31",
    ///     "activities": [
    ///         {
    ///             "name": "Act1",
    ///             "inverseActivityHeader": [
    ///                 {
    ///                     "name": "Act1.1",
    ///                     "inverseActivityHeader": []
    ///                 }
    ///             ]
    ///         },
    ///         {
    ///             "name": "Act2",
    ///             "inverseActivityHeader": [
    ///                 {
    ///                     "name": "Act2.1",
    ///                     "inverseActivityHeader": []
    ///                 },
    ///                 {
    ///                     "name": "Act2.2",
    ///                     "inverseActivityHeader": [
    ///                         {
    ///                             "name": "Act2.2.1",
    ///                             "inverseActivityHeader": []
    ///                         }
    ///                     ]
    ///                 }
    ///             ]
    ///         }
    ///     ]
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
    ///     "Data": {
    ///         "id": 1,
    ///         "name": "Project123",
    ///         "startDate": "2022-01-01",
    ///         "endDate": "2022-01-31",
    ///         "createDate": "2024-05-07T11:55:57.0455498+07:00",
    ///         "updateDate": "2024-05-07T11:55:57.0455541+07:00",
    ///         "isDelete": false,
    ///         "activities": []
    ///     }
    /// }
    /// ```
    /// </response>
    /// <response code="400">
    /// Bad Request
    ///  ```json
    /// {
    ///     "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    ///     "title": "One or more validation errors occurred.",
    ///     "status": 400,
    ///     "errors": {
    ///     "$.startDate": [
    ///         "The JSON value could not be converted to System.Nullable`1[System.DateOnly]. Path: $.startDate | LineNumber: 2 | BytePositionInLine: 16."
    ///         ]
    ///     },
    ///     "traceId": "00-b5c199ff556b804f91d212f9d2c16a07-68ca9d9ef8b71531-00"
    ///     }
    /// ```
    /// </response>
    /// <response code="500">
    /// Internal Server Error
    ///  ```json
    /// {
    ///     "Code": 500,
    ///     "Message": "Internal Server Error : An error occurred while saving the entity changes. See the inner exception for details.",
    ///     "Data": null
    /// }
    /// ```
    /// </response>
    [HttpPost(Name = "CreateProject")]
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
            Activity.SetActivities(project, project.Activities, projectCreate.Activities);

            // หลังจากที่สร้างกิจกรรมทั้งหมดแล้ว จึงทำการเพิ่มโปรเจคลงในฐานข้อมูล แล้วส่งกลับไปยัง Response กลับไปยัง Client
            Project.Create(_db, project);

            return Ok(new Response
            {
                Code = 200,
                Message = "Success",
                Data = project
            });

        }
        catch (Exception e)
        {
            return new Response
            {
                Code = 500,
                Message = "Internal Server Error : " + e.Message,
                Data = null
            };
        }
    }

    /// <summary>
    /// Get All
    /// </summary>
    [HttpGet(Name = "GetAll")]
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
    /// <returns></returns>
    /// <response code="200">
    /// Success
    ///  ```json
    /// {
    ///     "Code": 200,
    ///     "Message": "Success",
    ///     "Data": {
    ///         "id": 1,
    ///         "name": "Project123",
    ///         "startDate": "2022-01-01",
    ///         "endDate": "2022-01-31",
    ///         "createDate": "2024-05-07T11:55:57.0455498+07:00",
    ///         "updateDate": "2024-05-07T11:55:57.0455541+07:00",
    ///         "isDelete": false,
    ///         "activities": []
    ///     }
    /// }
    /// ```
    /// </response>
    /// <response code="400">Bad Request</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet("{id}", Name = "GetProjectById")]
    public ActionResult GetProjectById(int id)
    {
        Project project = Project.GetById(_db, id);

        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = project
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
    ///     "name": "Project123",
    ///     "startDate": "2022-01-01",
    ///     "endDate": "2022-01-31",
    ///     "createDate": "2024-05-07T11:55:57.0455498+07:00",
    ///     "updateDate": "2024-05-07T11:55:57.0455541+07:00",
    ///     "isDelete": false,
    ///     "activities": []
    /// }
    /// ```
    /// </remarks>
    /// <returns></returns>
    [HttpPut(Name = "UpdateProject")]
    public ActionResult UpdateProject(Project project)
    {
        project = Project.Update(_db, project);
        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = project
        });
    }

    /// <summary>
    /// Delete ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <response code="200">
    /// Success
    ///  ```json
    /// {
    ///     "Code": 200,
    ///     "Message": "Success",
    ///     "Data": {
    ///         "id": 1,
    ///         "name": "Project123",
    ///         "startDate": "2022-01-01",
    ///         "endDate": "2022-01-31",
    ///         "createDate": "2024-05-07T11:55:57.0455498+07:00",
    ///         "updateDate": "2024-05-07T11:55:57.0455541+07:00",
    ///         "isDelete": true,
    ///         "activities": []
    ///     }
    /// }
    /// ```
    /// </response>
    /// <response code="400">Bad Request</response>
    /// <response code="500">Internal Server Error</response>
    [HttpDelete("{id}", Name = "DeleteProjectById")]
    public ActionResult DeleteProjectById(int id)
    {
        Project project = Project.Delete(_db, id);
        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = project
        });
    }

}