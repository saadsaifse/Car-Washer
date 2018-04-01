using System;

namespace FirstScreen.CarWasher.Models
{
    public class Statistics
    {
        public int GeneratedVisitors { get; set; }
        public int RejectedVisitors { get; set; }
        public int ProcessedVisitors { get; set; }
        public TimeSpan AverageProcessingTime { get; set; }
        public TimeSpan AverageWaitingTime { get; set; }
        public TimeSpan AverageTotalTime { get; set; }
    }
}
