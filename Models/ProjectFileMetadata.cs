using activityCore.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace activityCore.Models
{
    public class ProjectFileMetadata { }

    [MetadataType(typeof(ProjectFileMetadata))]
    public partial class ProjectFile
    {
        //Create Action
        public static ProjectFile Create(ActivityContext db, ProjectFile projectFile)
        {
            projectFile.CreateDate = DateTime.Now;
            projectFile.UpdateDate = DateTime.Now;
            projectFile.IsDelete = false;
            db.ProjectFiles.Add(projectFile);
            db.SaveChanges();

            return projectFile;
        }

        public static ProjectFile GetById(ActivityContext db, int id)
        {
            ProjectFile? result = db.ProjectFiles.Where(q => q.Id == id && q.IsDelete != true).FirstOrDefault();
            return result ?? new ProjectFile();
        }

        public static ProjectFile Update(ActivityContext db, ProjectFile projectFile)
        {
            projectFile.UpdateDate = DateTime.Now;
            db.Entry(projectFile).State = EntityState.Modified;
            db.SaveChanges();

            return projectFile;
        }
    }
}