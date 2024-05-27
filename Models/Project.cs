﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace activityCore.Models;

public partial class Project
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public bool? IsDelete { get; set; }

    public virtual ICollection<Activity>? Activities { get; set; } = new List<Activity>();

    // [JsonIgnore] // property ที่จะถูก serialize
    public virtual ICollection<FileXproject>? FileXprojects { get; set; } = new List<FileXproject>();
}
