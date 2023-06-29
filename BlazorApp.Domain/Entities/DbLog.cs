using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp.Domain.Entities;

public class DbLog
{
    public string Message { get; set; }

    public string MessageTemplate { get; set; }

    public int Level { get; set; }

    public DateTimeOffset TimeStamp { get; set; }

    public string Exception { get; set; }

    public string Properties { get; set; }

    [NotMapped] public IDictionary<string, string> LogProperties { get; set; }
}