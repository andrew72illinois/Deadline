using System;
using System.Text.Json.Serialization;

namespace Deadline.Models
{
    public class Note
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string Content { get; set; } = string.Empty;
        public double? ProgressAmount { get; set; } // Optional: amount of progress made (for quantitative goals)
    }
}

