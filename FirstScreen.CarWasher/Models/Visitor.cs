using System;
using static FirstScreen.CarWasher.Enums.Enum;

namespace FirstScreen.CarWasher.Models
{
    public class Visitor
    {
        public string Id { get; set; }
        public DateTimeOffset GeneratedOn { get; set; }
        public DateTimeOffset? ProcessedOn { get; set; }
        public VisitorStatus  Status { get; set; }
        public TimeSpan ProcessingDuration { get; set; }
    }
}
