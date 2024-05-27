using activityCore.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace activityCore.Models
{
    public class UserMetadata { }

    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
        // Create Action
        public static User Create(ActivityContext db, User user)
        {
            user.Role = user.Role;
            user.CreateDate = DateTime.Now;
            user.UpdateDate = DateTime.Now;
            user.IsDelete = false;
            db.Users.Add(user);
            db.SaveChanges();

            return user;
        }

        //Get All Action
        public static List<User> GetAll(ActivityContext db)
        {
            List<User> users = db.Users.Where(q => q.IsDelete != true).ToList();
            return users;
        }

        //Get ID Action
        public static User GetById(ActivityContext db, int id)
        {
            User? result = db.Users.Where(q => q.Id == id && q.IsDelete != true).FirstOrDefault();
            return result ?? new User();
        }

        //Update Action
        public static User Update(ActivityContext db, User user)
        {
            user.UpdateDate = DateTime.Now;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();

            return user;
        }

        //Delete Action
        public static User Delete(ActivityContext db, int id)
        {
            User user = GetById(db, id);
            user.IsDelete = true;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();

            return user;
        }


    }


}