using Microsoft.AspNetCore.Mvc;
using activityCore.Data;
using activityCore.Models;


[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ActivityController : ControllerBase
{
    private ActivityContext _db = new ActivityContext();

    private readonly ILogger<ActivityController> _logger;

    public ActivityController(ILogger<ActivityController> logger)
    {
        _logger = logger;
    }

    public partial class ActivityCreateRequest
    {
        public int? ProjectId { get; set; }
        public int? ActivityHeaderId { get; set; }
        public string? Name { get; set; }
        public virtual ICollection<Activity> InverseActivityHeader { get; set; } = new List<Activity>();
    }

    /// <summary>
    /// Create Activity
    /// </summary>
    /// <remarks>
    /// Sample request :
    /// ```json
    /// [
    ///     {
    ///         "name": "Act1",
    ///         "projectId": 2,
    ///         "inverseActivityHeader": [
    ///             {
    ///                 "name": "Act1.1",
    ///                 "projectId": 2
    ///             }
    ///         ]
    ///     },
    ///     {
    ///         "name": "Act2",
    ///         "projectId": 2,
    ///         "inverseActivityHeader": [
    ///             {
    ///                 "name": "Act2.1",
    ///                 "projectId": 2
    ///             },
    ///             {
    ///                 "name": "Act2.2",
    ///                 "projectId": 2,
    ///                 "inverseActivityHeader": [
    ///                     {
    ///                         "name": "Act2.2.1",
    ///                         "projectId": 2
    ///                     }
    ///                 ]
    ///             }
    ///         ]
    ///     }
    /// ]
    /// ```
    /// </remarks>
    /// <returns></returns>
    /// <response code="200">
    /// Success
    /// <br/>
    /// <br/>
    ///  ```json
    ///  {
    ///     "code": 200,
    ///     "message": "Success",
    ///     "data": [
    ///         {
    ///             "id": 1,
    ///             "projectId": 2,
    ///             "activityHeaderId": null,
    ///             "name": "Act1",
    ///             "createDate": "2024-05-07T14:59:05.040895+07:00",
    ///             "updateDate": "2024-05-07T14:59:05.0408951+07:00",
    ///             "isDelete": false,
    ///             "activityHeader": null,
    ///             "inverseActivityHeader": [
    ///                 {
    ///                     "id": 2,
    ///                     "projectId": 2,
    ///                     "activityHeaderId": 1,
    ///                     "name": "Act1.1",
    ///                     "createDate": "2024-05-07T14:59:05.0408901+07:00",
    ///                     "updateDate": "2024-05-07T14:59:05.0408902+07:00",
    ///                     "isDelete": false,
    ///                     "activityHeader": null,
    ///                     "inverseActivityHeader": [],
    ///                     "project": null
    ///                 }
    ///             ],
    ///             "project": null
    ///         },
    ///         {
    ///             "id": 3,
    ///             "projectId": 2,
    ///             "activityHeaderId": null,
    ///             "name": "Act2",
    ///             "createDate": "2024-05-07T14:59:05.0842085+07:00",
    ///             "updateDate": "2024-05-07T14:59:05.0842085+07:00",
    ///             "isDelete": false,
    ///             "activityHeader": null,
    ///             "inverseActivityHeader": [
    ///                 {
    ///                     "id": 4,
    ///                     "projectId": 2,
    ///                     "activityHeaderId": 3,
    ///                     "name": "Act2.1",
    ///                     "createDate": "2024-05-07T14:59:05.0842055+07:00",
    ///                     "updateDate": "2024-05-07T14:59:05.0842056+07:00",
    ///                     "isDelete": false,
    ///                     "activityHeader": null,
    ///                     "inverseActivityHeader": [],
    ///                     "project": null
    ///                 },
    ///                 {
    ///                     "id": 5,
    ///                     "projectId": 2,
    ///                     "activityHeaderId": 3,
    ///                     "name": "Act2.2",
    ///                     "createDate": "2024-05-07T14:59:05.084207+07:00",
    ///                     "updateDate": "2024-05-07T14:59:05.084207+07:00",
    ///                     "isDelete": false,
    ///                     "activityHeader": null,
    ///                     "inverseActivityHeader": [
    ///                         {
    ///                             "id": 6,
    ///                             "projectId": 2,
    ///                             "activityHeaderId": 5,
    ///                             "name": "Act2.2.1",
    ///                             "createDate": "2024-05-07T14:59:05.0842074+07:00",
    ///                             "updateDate": "2024-05-07T14:59:05.0842075+07:00",
    ///                             "isDelete": false,
    ///                             "activityHeader": null,
    ///                             "inverseActivityHeader": [],
    ///                             "project": null
    ///                         }
    ///                     ],
    ///                     "project": null
    ///                 }
    ///             ],
    ///             "project": null
    ///         }
    ///     ]
    /// }
    /// ```
    /// </response>
    /// <response code="400">Bad Request</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost(Name = "CreateActivity")]
    public ActionResult<Response> Create(List<ActivityCreateRequest> activitiesRequest)
    {
        List<Activity> activities = new List<Activity>();
        activities = activitiesRequest.Select(a => new Activity
        {
            ProjectId = a.ProjectId,
            ActivityHeaderId = a.ActivityHeaderId,
            Name = a.Name,
            InverseActivityHeader = a.InverseActivityHeader
        }).ToList();
        try
        {
            foreach (Activity activity in activities)
            {
                Activity.SetActivitiesCreate(activity);
                Activity.Create(_db, activity);
            }
            _db.SaveChanges();
            return new Response
            {
                Code = 200,
                Message = "Success",
                Data = activities
            };
        }
        catch
        {
            return new Response
            {
                Code = 500,
                Message = "Internal Server Error",
                Data = null
            };
        }
    }


    /// <summary>
    /// Get All
    /// </summary>
    /// <remarks></remarks>
    /// <returns></returns>
    [HttpGet(Name = "GetAllActivities")]
    public ActionResult GetAllActivities()
    {
        List<Activity> activities = Activity.GetAll(_db).OrderBy(q => q.Id).ToList(); // ตามลำดับ

        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = activities
        });
    }

    /// <summary>
    /// Get ID
    /// </summary>
    /// <remarks></remarks>
    /// <returns></returns>
    [HttpGet("{id}", Name = "GetIdProject")]
    public ActionResult GetIdProject(int id)
    {
        Activity activity = Activity.GetById(_db, id);
        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = activity
        });
    }

    /// <summary>
    /// Update Activity
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// ```json
    /// {
    ///     "id": 1,
    ///     "projectId": 2,
    ///     "name": "Act1 Updated",
    ///     "createDate": "2024-05-07T22:56:31.513",
    ///     "updateDate": "2024-05-07T22:56:31.513",
    ///     "isDelete": true,
    ///     "inverseActivityHeader": [
    ///         {
    ///             "id": 2,
    ///             "projectId": 2,
    ///             "activityHeaderId": 1,
    ///             "name": "Act1.1 Updated",
    ///             "createDate": "2024-05-07T22:56:31.513",
    ///             "updateDate": "2024-05-07T22:56:31.513",
    ///             "isDelete": true,
    ///             "inverseActivityHeader": []
    ///         }
    ///     ]
    /// }
    /// ```
    /// </remarks>
    /// <returns></returns>
    /// <response code="200">Success</response>
    /// <response code="400">Bad Request</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPut(Name = "UpdateActivity")]
    public ActionResult UpdateActivity(Activity activity)
    {
        activity.UpdateDate = DateTime.Now;
        _db.Activities.Update(activity);
        _db.SaveChanges();


        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = activity

        });
    }


}