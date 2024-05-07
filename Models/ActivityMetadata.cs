using activityCore.Data;
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
            db.SaveChanges();

            return activity;
        }

        public static void SetActivitiesCreate(Activity activity)
        {
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
            Activity? activity = db.Activities.FirstOrDefault(q => q.Id == id && q.IsDelete != true);
            return activity ?? new Activity();
        }

        // Update
        public static Activity Update(ActivityContext db, Activity activity)
        {
            // Get the original activity from the database
            Activity oldActivity = GetById(db, activity.Id);

            oldActivity.ProjectId = activity.ProjectId;
            oldActivity.ActivityHeaderId = activity.ActivityHeaderId;
            oldActivity.Name = activity.Name;
            oldActivity.CreateDate = activity.CreateDate;
            oldActivity.UpdateDate = DateTime.Now;
            oldActivity.IsDelete = activity.IsDelete;

            db.SaveChanges();

            return oldActivity;
        }

    }


}