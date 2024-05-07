using activityCore.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

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

        // Get All
        public static List<Project> GetAll(ActivityContext db)
        {
            List<Project> projects = db.Projects.Where(e => e.IsDelete != true).ToList();
            return projects;
        }

        //Get ID
        public static Project GetById(ActivityContext db, int id)
        {
            Project? project = db.Projects.Where(q => q.Id == id && q.IsDelete != true).FirstOrDefault();
            return project ?? new Project();
        }

        //  Update
        public static Project Update(ActivityContext db, Project project)
        {
            project.UpdateDate = DateTime.Now;

            db.Entry(project).State = EntityState.Modified;
            // db.Projects.Update(project);

            db.SaveChanges();
            return project;
        }

        //  Delete ID
        public static Project Delete(ActivityContext db, int id)
        {
            Project project = GetById(db, id);
            project.IsDelete = true;
            db.Entry(project).State = EntityState.Modified;

            // db.Projects.Remove(project); // ลบในฐานข้อมูล

            db.SaveChanges();

            return project;
        }
    }


}