using activityCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace activityCore.Models
{
    public class ActivityMetadata
    {

    }

    [MetadataType(typeof(ActivityMetadata))]
    public partial class Activity
    {
        //Create Action
        public static Activity Create(ActivityContext db, Activity activity)
        {
            activity.CreateDate = DateTime.Now;
            activity.UpdateDate = DateTime.Now;
            activity.IsDelete = false;
            db.Activities.Add(activity);

            return activity;
        }

        // Action SetActivitiesCreate สำหรับวนใส่ค่า CreateDate, UpdateDate, IsDelete ให้กับตัวลูก
        public static void SetActivitiesCreate(Activity activity)
        {
            activity.CreateDate = DateTime.Now;
            activity.UpdateDate = DateTime.Now;
            activity.IsDelete = false;

            foreach (Activity subActivity in activity.InverseActivityHeader)
            {
                SetActivitiesCreate(subActivity);
            }
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
            Activity? activity = db.Activities
                .Include(a => a.ActivityHeader)
                    .ThenInclude(a => a.InverseActivityHeader)
                        .ThenInclude(a => a.InverseActivityHeader)
                .Include(a => a.ActivityHeader)
                    .ThenInclude(a => a.ActivityHeader)
                        .ThenInclude(a => a.InverseActivityHeader)
                .Include(a => a.InverseActivityHeader)
                    .ThenInclude(a => a.InverseActivityHeader)
                .FirstOrDefault(q => q.Id == id && q.IsDelete != true);

            return activity ?? new Activity();
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
            }
            return oldActivity;
        }
    }
}