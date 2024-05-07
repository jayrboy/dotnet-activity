using System;
using System.Collections.Generic;
using activityCore.Data;

namespace activityCore.Models;

public partial class Activity
{
    public int Id { get; set; }

    public int? ProjectId { get; set; }

    public int? ActivityHeaderId { get; set; }

    public string? Name { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public bool? IsDelete { get; set; }

    public virtual Activity? ActivityHeader { get; set; }

    public virtual ICollection<Activity>? InverseActivityHeader { get; set; } = new List<Activity>();

    public virtual Project? Project { get; set; }

    internal static List<Activity> Update(ActivityContext db, List<Activity> activity)
    {
        throw new NotImplementedException();
    }
}
