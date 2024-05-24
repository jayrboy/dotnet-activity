using Microsoft.AspNetCore.Mvc;
using activityCore.Data;
using activityCore.Models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace activityCore.Controllers
{
    // [Authorize(Roles = "admin, user")]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProjectController : ControllerBase
    {
        private ActivityContext _db = new ActivityContext();

        private IHostEnvironment _hostingEnvironment;

        private readonly ILogger<ProjectController> _logger;

        public ProjectController(ILogger<ProjectController> logger, IHostEnvironment environment)
        {
            _logger = logger;
            _hostingEnvironment = environment;
        }

        public struct ProjectCreate
        {
            /// <summary>
            /// Project Name
            /// </summary>
            /// <example>Project123</example>
            /// <required>true</required>
            [Required]
            // [System.Text.Json.Serialization.JsonPropertyName("name")]
            public string? Name { get; set; }

            /// <summary>
            /// Start Date
            /// </summary>
            /// <example>2022-01-01</example>
            // [System.Text.Json.Serialization.JsonPropertyName("startDate")]
            public DateOnly? StartDate { get; set; }

            /// <summary>
            /// Update Date
            /// </summary>
            /// <example>2022-01-31</example>
            // [System.Text.Json.Serialization.JsonPropertyName("endDate")]
            public DateOnly? EndDate { get; set; }

            /// <summary>
            /// Activities of Project
            /// </summary>
            /// <example>["string"]</example>
            // [System.Text.Json.Serialization.JsonPropertyName("activities")]
            public List<Activity>? Activities { get; set; }
        }

        public class ProjectFileCreate
        {
            public string projectCreate { get; set; }
            public List<IFormFile>? fromFile { get; set; }
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
        public ActionResult<Response> CreateProject([FromForm] ProjectFileCreate projectFileCreate)
        {
            //(2) [FromFrom] string Convert to (Json Serializer + Options)
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            ProjectCreate? projectJson = JsonSerializer.Deserialize<ProjectCreate>(projectFileCreate.projectCreate, options);

            //(3) Save Project Table
            Project project = new Project
            {
                Name = projectJson.Value.Name,
                StartDate = projectJson.Value.StartDate,
                EndDate = projectJson.Value.EndDate,
            };

            try
            {
                Activity.SetActivitiesCreate(project, project.Activities, projectJson.Value.Activities);

                // หลังจากที่สร้างกิจกรรมทั้งหมดแล้ว ก็เพิ่มลงในฐานข้อมูล
                Project.Create(_db, project);

                //(4) [FromForm] List<fromFile> 
                foreach (IFormFile f in projectFileCreate.fromFile)
                {
                    Models.File file = new Models.File
                    {
                        FileName = f.FileName,
                        FilePath = "UploadedFile/ProjectFile/"
                    };
                    //(5) Save File Table
                    Models.File.Create(_db, file);

                    if (f != null && f.Length > 0)
                    {
                        string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "UploadedFile/ProjectFile/" + file.Id);

                        Directory.CreateDirectory(uploads);
                        string filePath = Path.Combine(uploads, f.FileName);

                        using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            f.CopyTo(fileStream);
                        }
                    }

                    FileXproject fileXproject = new FileXproject
                    {
                        ProjectId = project.Id,
                        FileId = file.Id,
                    };
                    //(6) Save FileXproject Table
                    FileXproject.Create(_db, fileXproject);
                }
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
                Data = projectFileCreate
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

            // var response = new
            // {
            //     Name = project.Name,
            //     StartDate = project.StartDate,
            //     EndDate = project.EndDate,
            //     Activities = project.Activities,
            //     File = project.File,
            // };

            return project == null
            ? NotFound(new Response
            {
                Code = 404,
                Message = "Not Found",
                Data = null
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
        public ActionResult UpdateProject([FromForm] Project project)
        {
            try
            {
                project = Project.Update(_db, project);
                // Add File
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

        /* ---------------------------------------- Testing (Response, Request) ---------------------------------------- */
        //(1) Define [FromForm] to parameter Creating by Model "ProjectFileCreate" 

        [HttpPost("Test", Name = "TestApi")]
        public ActionResult TestRequest([FromForm] ProjectFileCreate projectFileCreate)
        {
            //(2) [FromFrom] string Convert to (Json Serializer + Options)
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            ProjectCreate? projectJson = JsonSerializer.Deserialize<ProjectCreate>(projectFileCreate.projectCreate, options);

            //(3) Save Project Table
            Project project = new Project
            {
                Name = projectJson.Value.Name,
                StartDate = projectJson.Value.StartDate,
                EndDate = projectJson.Value.EndDate,
            };

            try
            {
                Activity.SetActivitiesCreate(project, project.Activities, projectJson.Value.Activities);

                // หลังจากที่สร้างกิจกรรมทั้งหมดแล้ว ก็เพิ่มลงในฐานข้อมูล
                Project.Create(_db, project);

                //(4) [FromForm] List<fromFile> 
                foreach (IFormFile f in projectFileCreate.fromFile)
                {
                    Models.File file = new Models.File
                    {
                        FileName = f.FileName,
                        FilePath = "UploadedFile/ProjectFile/"
                    };
                    //(5) Save File Table
                    Models.File.Create(_db, file);

                    if (f != null && f.Length > 0)
                    {
                        string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "UploadedFile/ProjectFile/" + file.Id);

                        Directory.CreateDirectory(uploads);
                        string filePath = Path.Combine(uploads, f.FileName);

                        using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            f.CopyTo(fileStream);
                        }
                    }

                    FileXproject fileXproject = new FileXproject
                    {
                        ProjectId = project.Id,
                        FileId = file.Id,
                    };
                    //(6) Save FileXproject Table
                    FileXproject.Create(_db, fileXproject);
                }
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
                Data = projectFileCreate
            });
        }
        /* ---------------------------------------- Testing (Response, Request) ---------------------------------------- */

    }
}