using activityCore.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace activityCore.Models
{
    public class AccountMetadata { }

    [MetadataType(typeof(AccountMetadata))]
    public partial class Account
    {
        // Create Action
        public static Account Create(ActivityContext db, Account user)
        {
            user.Role = user.Role;
            user.CreateDate = DateTime.Now;
            user.UpdateDate = DateTime.Now;
            user.IsDelete = false;
            db.Accounts.Add(user);
            db.SaveChanges();

            return user;
        }

        //Get All Action
        public static List<Account> GetAll(ActivityContext db)
        {
            List<Account> users = db.Accounts.Where(q => q.IsDelete != true).ToList();
            return users;
        }

        //Get ID Action
        public static Account GetById(ActivityContext db, int id)
        {
            Account? result = db.Accounts.Where(q => q.Id == id && q.IsDelete != true).FirstOrDefault();
            return result ?? new Account();
        }

        //Update Action
        public static Account Update(ActivityContext db, Account user)
        {
            user.UpdateDate = DateTime.Now;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();

            return user;
        }

        //Delete Action
        public static Account Delete(ActivityContext db, int id)
        {
            Account user = GetById(db, id);
            user.IsDelete = true;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();

            return user;
        }


    }


}