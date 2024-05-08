using activityCore.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

namespace activityCore.Models
{
    public class UserMetadata
    {

    }

    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
        // Create Action
        public static User Create(ActivityContext db, User user)
        {
            db.Users.Add(user);
            db.SaveChanges();

            return user;
        }
    }


}