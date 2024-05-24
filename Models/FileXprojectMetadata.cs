using activityCore.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace activityCore.Models
{
    public class FileXprojectMetadata { }

    [MetadataType(typeof(FileXprojectMetadata))]
    public partial class FileXproject
    {
        //Create Action
        public static FileXproject Create(ActivityContext db, FileXproject fileXproject)
        {
            fileXproject.CreateDate = DateTime.Now;
            fileXproject.UpdateDate = DateTime.Now;
            fileXproject.IsDelete = false;
            db.FileXprojects.Add(fileXproject);
            db.SaveChanges();

            return fileXproject;
        }
    }
}