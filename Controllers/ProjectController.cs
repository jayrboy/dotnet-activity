using Microsoft.AspNetCore.Mvc;
using activityCore.Data;
using activityCore.Models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace activityCore.Controllers
{
    // [Authorize(Roles = "admin, user")]
    [ApiController]
    [Route("api/[controller]")]
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

                return Ok(new Response
                {
                    Code = 200,
                    Message = "Success",
                    Data = projectFileCreate
                });
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
            List<Project> projects = Project.GetAll(_db); // ตามลำดับ

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
        public ActionResult UpdateProject([FromBody] Project project)
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
        [HttpPost("Test")]
        public ActionResult<Response> TestApiUpdate([FromForm] string project, List<IFormFile> formFile)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            Project? projectJson = JsonSerializer.Deserialize<Project>(project, options);

            // //TODO: แก่ไข หรือ เพิ่มไฟล์เพิ่มจาก project.File ของเก่า และกรณีมีเพิ่มเข้ามาจาก formFile
            // รับข้อมูล FromForm เอา product ID ไปหาข้อมูลโครงการเก่า
            Project? updateProject = Project.GetById(_db, projectJson.Id);

            // นำข้อมูลที่หา มากำหนดข้อมูลใหม่ เพื่ออัปเดทข้อมูลโครงการ
            updateProject.Name = projectJson.Name;
            updateProject.StartDate = projectJson.StartDate;
            updateProject.EndDate = projectJson.EndDate;
            updateProject.UpdateDate = DateTime.Now;
            updateProject.IsDelete = projectJson.IsDelete;

            Activity.SetActivitiesCreate(updateProject, updateProject.Activities, projectJson.Activities);


            //TODO: แก้ไขไฟล์ที่มีอยู่ในโครงการ (จาก project เก่าที่ส่งมาแก้ไข)
            foreach (Models.File f in projectJson.File.Where(q => q.IsDelete == true))
            {
                Models.File existingFile = Models.File.GetById(_db, f.Id);

                if (existingFile != null && existingFile.IsDelete != true)
                {
                    existingFile.IsDelete = f.IsDelete;
                    Models.File.Update(_db, existingFile);
                }
            }

            //TODO: เพิ่มไฟล์ใหม่ (จาก formFile)
            foreach (IFormFile f in formFile)
            {
                Models.File newFile = new Models.File
                {
                    FileName = f.FileName,
                    FilePath = "UploadedFile/ProjectFile/",
                };

                Models.File.Create(_db, newFile); // เพิ่มข้อมูลเข้า File Table เพื่อที่จะเอา Id

                if (f.Length > 0)
                {
                    string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "UploadedFile/ProjectFile/" + newFile.Id);
                    Directory.CreateDirectory(uploads);

                    string filePath = Path.Combine(uploads, f.FileName);
                    using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        f.CopyTo(fileStream);
                    }
                }

                FileXproject fileXproject = new FileXproject
                {
                    ProjectId = updateProject.Id,
                    FileId = newFile.Id
                };

                FileXproject.Create(_db, fileXproject);
            }

            Project.Update(_db, updateProject);

            return Ok(new Response
            {
                Code = 200,
                Message = "Success",
            });
        }
        /* ---------------------------------------- Testing (Response, Request) ---------------------------------------- */
    }
}