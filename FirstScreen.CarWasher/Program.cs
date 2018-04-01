using Microsoft.Extensions.Configuration;
using FirstScreen.CarWasher.Models;
using System;
using System.Linq;

namespace FirstScreen.CarWasher
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsetting.json", true, true)
            .Build();

            var carWasher = new CarWasher(config);
            carWasher.Initialize();
            System.Threading.Timer visitorTimer = new System.Threading.Timer(OnVisitorArrived, carWasher, TimeSpan.Zero, new TimeSpan(0, 0, Int32.Parse(config["VisitorArrivalTimeGapSeconds"])));

            Console.WriteLine("Press ESC to stop and S for statistics.");

            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.S)
                {
                    var stats = GetStatistics(carWasher);
                    Console.WriteLine($"Total Generated Visitors = {stats.GeneratedVisitors}");
                    Console.WriteLine($"Total Processed Visitors = {stats.ProcessedVisitors}");
                    Console.WriteLine($"Total Rejected Visitors = {stats.RejectedVisitors}");
                    Console.WriteLine($"Average Processing Time = {stats.AverageProcessingTime}");
                    Console.WriteLine($"Average Waiting Time = {stats.AverageWaitingTime}");
                    Console.WriteLine($"Average Total Time = {stats.AverageTotalTime}");
                }
                else if (key == ConsoleKey.Escape)
                    break;
            }
            visitorTimer.Dispose();
        }

        private static Statistics GetStatistics(CarWasher carWasher)
        {
            try
            {
                var visitors = carWasher.GetAllVisitors();

                var processedVisitors = visitors.Where(v => v.ProcessedOn.HasValue && v.Status == Enums.Enum.VisitorStatus.Processed);

                TimeSpan averageProcessingTime, averageTotalTime, averageWaitingTime;
                if (processedVisitors.Any())
                {
                    averageTotalTime = TimeSpan.FromMilliseconds(processedVisitors.Average(v => (v.ProcessedOn.Value - v.GeneratedOn).TotalMilliseconds));
                    averageWaitingTime = TimeSpan.FromMilliseconds(processedVisitors.Average(v => ((v.ProcessedOn.Value - v.GeneratedOn) - v.ProcessingDuration).TotalMilliseconds));
                    averageProcessingTime = averageTotalTime - averageWaitingTime;
                }

                var statistics = new Statistics
                {
                    GeneratedVisitors = visitors.Count(),
                    ProcessedVisitors = processedVisitors.Count(),
                    RejectedVisitors = visitors.Count(v => v.Status == Enums.Enum.VisitorStatus.Rejected),
                    AverageProcessingTime = averageProcessingTime,
                    AverageTotalTime = averageTotalTime,
                    AverageWaitingTime = averageWaitingTime
                };

                return statistics;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error getting statistics. Exception: {ex.Message}");
                return null;
            }
        }

        private static void OnVisitorArrived(object state)
        {
            try
            {
                if (state is CarWasher washer)
                    washer.CreateVisitor();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
