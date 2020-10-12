using System;
using System.Collections.Generic;

namespace BlazorApp.Common.Models
{
    public class DbLogModel
    {
        public string Message { get; set; }

        public string MessageTemplate { get; set; }

        public int Level { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string Exception { get; set; }

        public string Properties { get; set; }

        public IDictionary<string, string> LogProperties { get; set; }
    }
}