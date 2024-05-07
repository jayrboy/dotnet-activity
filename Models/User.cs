using System;
using System.Collections.Generic;

namespace activityCore.Models;

public partial class User
{
    public int Id { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Token { get; set; }
}
