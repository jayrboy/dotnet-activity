using activityCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace activityCore.Models
{
    public class ProjectMetadata { }

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

            foreach (Project p in projects)
            {
                List<Activity> activities = db.Activities.Where(q => q.IsDelete != true).ToList();
                p.Activities = activities;
            }

            return projects;
        }

        //Get ID
        public static Project GetById(ActivityContext db, int id)
        {
            Project? project = db.Projects.Where(q => q.Id == id && q.IsDelete != true).FirstOrDefault();

            // หากไม่พบโปรเจกต์ก็คืนค่าอ็อบเจกต์ Project ว่างเปล่า
            if (project == null)
            {
                return new Project();
            }

            // ดึงข้อมูลกิจกรรมสำหรับโปรเจกต์นี้
            List<Activity> activities = db.Activities.Where(a => a.ProjectId == id && a.ActivityHeaderId == null && a.IsDelete != true)
            .Include(a => a.ActivityHeader)
                .ThenInclude(a => a.InverseActivityHeader)
                    .ThenInclude(a => a.InverseActivityHeader)
            .Include(a => a.ActivityHeader)
                .ThenInclude(a => a.ActivityHeader)
                    .ThenInclude(a => a.InverseActivityHeader)
            .Include(a => a.InverseActivityHeader)
                    .ThenInclude(a => a.InverseActivityHeader)
            .OrderBy(q => q.Id).ToList(); // ตามลำดับ

            // เก็บข้อมูลกิจกรรมลงในโปรเจกต์
            project.Activities = activities;

            return project ?? new Project();
        }

        //  Update
        public static Project Update(ActivityContext db, Project project)
        {
            Project oldProject = db.Projects.Include(p => p.Activities)
                                                .ThenInclude(a => a.InverseActivityHeader)
                                            .FirstOrDefault(p => p.Id == project.Id);

            // อัปเดตข้อมูลโปรเจกต์
            oldProject.Name = project.Name;
            oldProject.StartDate = project.StartDate;
            oldProject.EndDate = project.EndDate;
            oldProject.CreateDate = project.CreateDate;
            oldProject.UpdateDate = DateTime.Now;
            oldProject.IsDelete = project.IsDelete;

            Activity.UpdateActivities(project, oldProject.Activities, project.Activities);
            db.SaveChanges();

            return oldProject;
        }

        //  Delete ID
        public static Project Delete(ActivityContext db, int id)
        {
            Project project = GetById(db, id);
            project.IsDelete = true;
            db.Entry(project).State = EntityState.Modified;

            List<Activity> activities = db.Activities.Where(a => a.ProjectId == id).OrderBy(q => q.Id).ToList(); // ตามลำดับ

            foreach (Activity a in activities)
            {
                a.IsDelete = true;
            }
            // db.Projects.Remove(project); // ลบหายทันที 

            db.SaveChanges();

            return project;
        }
    }


}