using System;
using System.Collections.Generic;

namespace MyRazorApp.Models;

public partial class Class
{
    public int Id { get; set; }

    public string ClassName { get; set; } = null!;

    public int StudentCount { get; set; }

    public string Description { get; set; } = null!;

    public bool IsActive { get; set; }
}
