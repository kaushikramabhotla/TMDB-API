using System;
using System.Collections.Generic;

namespace TMDB_API.Models;

public partial class Credit
{
    public int MovieId { get; set; }

    public string Title { get; set; } = null!;

    public string? Cast { get; set; }

    public string? Crew { get; set; }
}
