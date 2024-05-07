using activityCore.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace activityCore.Models
{
    public class ProjectMetadata
    {

    }

    [MetadataType(typeof(ProjectMetadata))]
    public partial class Project
    {
        //Create Action
        public static Project Create(ActivityContext db, Project project)
        {
            project.CreateDate = DateTime.Now;
            project.UpdateDate = DateTime.Now;
            project.IsDelete = false;
            db.Projects.Add(project);
            db.SaveChanges();

            return project;
        }
    }
}