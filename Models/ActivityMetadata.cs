using activityCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace activityCore.Models
{
    public class ActivityMetadata { }

    //TODO: Actions of Project
    [MetadataType(typeof(ActivityMetadata))]
    public partial class Activity
    {
        public static Activity Create(ActivityContext db, Activity activity)
        {
            activity.CreateDate = DateTime.Now;
            activity.UpdateDate = DateTime.Now;
            activity.IsDelete = false;
            db.Activities.Add(activity);
            db.SaveChanges();

            return activity;
        }

        //TODO: สำหรับวนใส่ค่าให้กับตัวลูก. Method นี้เรียกใช้เมื่อสร้างกิจกรรมหลัก และ กิจกรรมย่อย
        public static void SetActivities(Activity activity)
        {
            activity.CreateDate = DateTime.Now;
            activity.UpdateDate = DateTime.Now;
            activity.IsDelete = false;

            foreach (Activity subActivity in activity.InverseActivityHeader)
            {
                subActivity.ProjectId = activity.ProjectId;
                SetActivities(subActivity);
            }
        }

        //TODO: สำหรับวนใส่ค่าให้กับตัวลูก. Method นี้เรียกใช้เมื่อสร้าง Project และ กิจกรรมต่างๆ
        public static void SetActivitiesCreate(Project project, ICollection<Activity> oldActivities, ICollection<Activity> newActivities)
        {
            foreach (Activity subActivity in newActivities)  // ไปวนหาค่าภายใน activity 
            {
                Activity newActivity = new Activity  // สร้าง activity ใหม่
                {
                    Name = subActivity.Name,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    IsDelete = false,
                    Project = project,
                };
                SetActivitiesCreate(project, newActivity.InverseActivityHeader, subActivity.InverseActivityHeader);
                oldActivities.Add(newActivity);
            }
        }

        //TODO: Update Project & Activities
        public static void SetActivitiesUpdate(Project project, ICollection<Activity> oldActivities, ICollection<Activity> newActivities)
        {
            foreach (Activity newActivity in newActivities)
            {
                Activity existingActivity = oldActivities.FirstOrDefault(a => a.Id == newActivity.Id);

                if (existingActivity != null)
                {
                    // ถ้ามีข้อมูลเก่า ก็อัปเดตข้อมูลของ activity ที่มีอยู่
                    existingActivity.Name = newActivity.Name;
                    existingActivity.UpdateDate = DateTime.Now;

                    // อัปเดตข้อมูลลูกของ activity นี้
                    SetActivitiesUpdate(project, existingActivity.InverseActivityHeader, newActivity.InverseActivityHeader);
                }
                else
                {
                    // ถ้าไม่มีข้อมูลเก่า ก็สร้าง activity ใหม่
                    Activity newAct = new Activity
                    {
                        Name = newActivity.Name,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        IsDelete = newActivity.IsDelete,
                        ProjectId = project.Id
                    };

                    // อัปเดตข้อมูลลูกของ activity ที่สร้างใหม่
                    SetActivitiesUpdate(project, newAct.InverseActivityHeader, newActivity.InverseActivityHeader);

                    // เพิ่ม activity ที่สร้างใหม่ลงในคอลเลกชันของกิจกรรมเก่า
                    oldActivities.Add(newAct);
                }
            }
        }

        public static Activity GetActivityRecursiveFn(Activity activity)
        {
            activity.InverseActivityHeader = activity.InverseActivityHeader
                .Where(a => a.IsDelete != true)
                .Select(a => GetActivityRecursiveFn(a))
                .ToList();
            return activity;
        }

        //Get All Action
        public static List<Activity> GetAll(ActivityContext db)
        {
            List<Activity> activities = db.Activities.Where(q => q.IsDelete != true).ToList();
            return activities;
        }

        //Get ID
        public static Activity GetById(ActivityContext db, int id)
        {
            Activity? returnThis = db.Activities.Where(q => q.Id == id && q.IsDelete != true).FirstOrDefault();
            return returnThis ?? new Activity();
        }

        // Update Main and Sub
        public static Activity Update(ActivityContext db, Activity activity)
        {
            Activity oldActivity = GetById(db, activity.Id);

            if (oldActivity.Name != activity.Name || oldActivity.IsDelete != activity.IsDelete)
            {
                oldActivity.Name = activity.Name;
                oldActivity.IsDelete = activity.IsDelete;
                oldActivity.UpdateDate = DateTime.Now;

                if (!oldActivity.InverseActivityHeader.IsNullOrEmpty())
                {
                    foreach (Activity? subActivity in activity.InverseActivityHeader)
                    {
                        subActivity.UpdateDate = DateTime.Now;
                        Update(db, subActivity); // Update the subActivity recursively
                    }
                }
                db.SaveChanges();
            }
            return oldActivity;
        }

        public static Activity Delete(ActivityContext db, int id)
        {
            Activity activity = GetById(db, id);
            activity.IsDelete = true;
            activity.UpdateDate = DateTime.Now;
            db.Entry(activity).State = EntityState.Modified;
            db.SaveChanges();

            return activity;
        }

    }
}