using activityCore.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace activityCore.Models
{
    public class ProjectMetadata { }

    [MetadataType(typeof(ProjectMetadata))]
    public partial class Project
    {
        [NotMapped] // ไม่สร้าง attribute นี้ในฐานข้อมูล
        public List<File>? File { get; set; }
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
            List<Project> projects = db.Projects.Where(e => e.IsDelete != true)
                                                .Include(p => p.Activities.Where(a => a.ActivityHeaderId == null && a.IsDelete != true))
                                                .Include(p => p.FileXprojects)
                                                .ToList();

            return projects;
        }

        //Get ID
        public static Project GetById(ActivityContext db, int id)
        {
            Project? project = db.Projects.Where(p => p.Id == id && p.IsDelete != true)
                                          .Include(p => p.Activities.Where(a => a.IsDelete != true))
                                            .ThenInclude(a => a.InverseActivityHeader.Where(sa => sa.IsDelete != true))
                                          .Include(p => p.FileXprojects)
                                            .ThenInclude(a => a.File)
                                          .FirstOrDefault();
            if (project == null)
            {
                return new Project();
            }
            else
            {
                project.File = new List<File>();

                foreach (FileXproject f in project.FileXprojects)
                {
                    project.File.Add(f.File);
                }

                project.Activities = project.Activities.Where(a => a.ActivityHeaderId == null && a.IsDelete != true)
                                                       .Select(a => Activity.GetActivityRecursiveFn(a)).ToList();
            }
            return project;
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

            // อัปเดตกิจกรรม
            Activity.SetActivitiesUpdate(project, oldProject.Activities, project.Activities);

            // SaveChanges เป็นการดำเนินการเก็บข้อมูลเข้า Database จะคืนค่า (Success, fail)
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