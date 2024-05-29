using Microsoft.AspNetCore.Mvc;
using activityCore.Data;
using activityCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace activityCore.Controllers;

// [Authorize]
[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private ActivityContext _db = new ActivityContext();
    private IHostEnvironment _hostingEnvironment;

    private readonly ILogger<FileController> _logger;

    public FileController(ILogger<FileController> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _hostingEnvironment = environment;
    }

    /// <summary>
    /// Upload
    /// </summary>
    /// <param name="formFile"></param>
    /// <remarks></remarks>
    /// <returns></returns>
    /// <response code="200">
    /// Success
    ///  ```json
    /// {
    ///     "Code": 200,
    ///     "Message": "Success",
    ///     "Data": {
    ///         "id": 1,
    ///         "fileName": "ArgusChibi.png",
    ///         "filePath": "UploadedFile/ProfileImg/",
    ///         "endDate": "2022-01-31",
    ///         "createDate": "2024-05-08T22:25:47.6367441+07:00",
    ///         "updateDate": "2024-05-08T22:25:47.6367441+07:00",
    ///         "isDelete": false,
    ///         "fileXprojects": []
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
    ///         "formFile": [
    ///             "The formFile field is required."
    ///         ]
    ///     },
    ///     "traceId": "00-24309c75a8ef82c57b4614e4867f029a-b57ae6289e61bab7-00"
    ///     }
    /// ```
    /// </response>
    [HttpPost(Name = "UploadFile")]
    public ActionResult UploadFile(List<IFormFile> formFile)
    {
        if (formFile == null)
        {
            return BadRequest(new Response
            {
                Code = 400,
                Message = "File is required"
            });
        }

        foreach (IFormFile f in formFile)
        {
            Models.File file = new Models.File
            {
                FileName = f.FileName,
                FilePath = "UploadedFile/ProfileImg/"
            };
            file = Models.File.Create(_db, file);

            if (f != null && f.Length > 0)
            {
                string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "UploadedFile/ProfileImg/" + file.Id);
                // string filePath = Path.Combine(Server.MapPath("~/UploadedFile/Profile/") + AccountExtensions.File.Id + "/" + file.FileName);

                Directory.CreateDirectory(uploads);
                string filePath = Path.Combine(uploads, f.FileName);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    f.CopyTo(fileStream);
                }
            }
        }

        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = formFile
        });
    }

    [HttpDelete("{id}", Name = "DeleteById")]
    public ActionResult<Response> DeleteFile(int id)
    {
        if (id != null)
        {
            Models.File file = Models.File.Delete(_db, id);

            return Ok(new Response
            {
                Code = 200,
                Message = "Success",
                Data = file
            });
        }
        else
        {
            return NotFound();
        }
    }

}