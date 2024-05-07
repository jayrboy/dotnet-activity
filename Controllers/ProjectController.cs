using Microsoft.AspNetCore.Mvc;
using activityCore.Data;
using activityCore.Models;

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
        public string? Name { get; set; }

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

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
    ///     "endDate": "2022-01-31"
    /// }
    /// ```
    /// </remarks>
    /// <param name="projectCreate"></param>
    /// <returns></returns>
    [HttpPost(Name = "CreateProject")]
    public ActionResult CreateProject(ProjectCreate projectCreate)
    {
        Project project = new Project
        {
            Name = projectCreate.Name,
            StartDate = projectCreate.StartDate,
            EndDate = projectCreate.EndDate,
        };

        project = Project.Create(_db, project);
        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = project
        });
    }



}