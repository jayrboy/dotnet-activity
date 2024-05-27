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

        public static FileXproject GetById(ActivityContext db, int id)
        {
            FileXproject? result = db.FileXprojects.Where(q => q.Id == id && q.IsDelete != true).FirstOrDefault();
            return result ?? new FileXproject();
        }

        public static FileXproject Update(ActivityContext db, FileXproject fileXproject)
        {
            fileXproject.UpdateDate = DateTime.Now;
            db.Entry(fileXproject).State = EntityState.Modified;
            db.SaveChanges();

            return fileXproject;
        }
    }
}