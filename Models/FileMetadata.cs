using System.ComponentModel.DataAnnotations;
using activityCore.Data;
using Microsoft.EntityFrameworkCore;

namespace activityCore.Models
{
    public class FileMetadata { }

    [MetadataType(typeof(FileMetadata))]
    public partial class File
    {
        // [NotMapped]
        // public File? File {get; set; }

        public static File Create(ActivityContext db, File file)
        {
            file.CreateDate = DateTime.Now;
            file.UpdateDate = DateTime.Now;
            file.IsDelete = false;
            db.Files.Add(file);
            db.SaveChanges();
            return file;
        }

        public static File GetById(ActivityContext db, int id)
        {
            File? file = db.Files.Where(q => q.Id == id && q.IsDelete != true).FirstOrDefault();

            return file;
        }

        public static File Update(ActivityContext db, File file)
        {
            file.UpdateDate = DateTime.Now;
            db.Entry(file).State = EntityState.Modified;
            db.SaveChanges();

            return file;
        }

        public static File Delete(ActivityContext db, int id)
        {
            File file = GetById(db, id);
            file.IsDelete = true;

            db.Entry(file).State = EntityState.Modified;
            db.SaveChanges();

            return file;
        }
    }


}