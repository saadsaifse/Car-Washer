using Microsoft.Extensions.Configuration;
using FirstScreen.CarWasher.Interfaces;
using FirstScreen.CarWasher.Managers.Bay;
using FirstScreen.CarWasher.Managers.Queue;
using FirstScreen.CarWasher.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FirstScreen.CarWasher
{
    public class CarWasher
    {
        ConcurrentDictionary<string, Visitor> Visitors;

        IConfiguration configuration;
        IQueueManager queueManager;
        IBayManager bayManager;
        public CarWasher(IConfiguration configuration)
        {
            this.configuration = configuration;
            Visitors = new ConcurrentDictionary<string, Visitor>();
        }

        public void Initialize()
        {
            queueManager = new QueueManager();

            var firstWashingQueue = queueManager.CreateQueue(Int32.Parse(configuration["FirstWashingBayQueueSize"]), Enums.Enum.QueueType.Washing);
            var secondWashingQueue = queueManager.CreateQueue(Int32.Parse(configuration["SecondWashingBayQueueSize"]), Enums.Enum.QueueType.Washing);
            var thirdWashingQueue = queueManager.CreateQueue(Int32.Parse(configuration["ThirdWashingBayQueueSize"]), Enums.Enum.QueueType.Washing);
            var dryingQueue = queueManager.CreateQueue(Int32.Parse(configuration["DryingBayQueueSize"]), Enums.Enum.QueueType.Drying);

            bayManager = new BayManager(queueManager);
            bayManager.Processed += BayManager_Processed;
            bayManager.ProcessTime += BayManager_ProcessTime;
            bayManager.CreateBay(firstWashingQueue, Enums.Enum.BayType.Washing, Int32.Parse(configuration["FirstWashingBayProcessingSecond"]));
            bayManager.CreateBay(secondWashingQueue, Enums.Enum.BayType.Washing, Int32.Parse(configuration["SecondWashingBayProcessingSecond"]));
            bayManager.CreateBay(thirdWashingQueue, Enums.Enum.BayType.Washing, Int32.Parse(configuration["ThirdWashingBayProcessingSecond"]));
            bayManager.CreateBay(dryingQueue, Enums.Enum.BayType.Drying, Int32.Parse(configuration["FirstDryingBayProcessingSecond"]));
            bayManager.CreateBay(dryingQueue, Enums.Enum.BayType.Drying, Int32.Parse(configuration["SecondDryingBayProcessingSecond"]));
        }

        private void BayManager_ProcessTime(object sender, Tuple<string, TimeSpan> e)
        {
            if (Visitors.TryGetValue(e.Item1, out Visitor visitor))
            {
                visitor.ProcessingDuration += e.Item2;
            }
        }

        private void BayManager_Processed(object sender, string e)
        {
            if (Visitors.TryGetValue(e, out Visitor visitor))
            {
                visitor.ProcessedOn = DateTimeOffset.Now;
                visitor.Status = Enums.Enum.VisitorStatus.Processed;
            }
        }

        public IEnumerable<Visitor> GetAllVisitors()
        {
            return Visitors.Values.ToList();
        }

        public void CreateVisitor()
        {
            var visitor = new Visitor
            {
                GeneratedOn = DateTimeOffset.Now,
                Id = Guid.NewGuid().ToString().Substring(0,3).ToUpper(),
                Status = Enums.Enum.VisitorStatus.Underprocessing
            };

            if (!queueManager.TryOptimalEnqueue(Enums.Enum.QueueType.Washing, visitor))
            {
                visitor.Status = Enums.Enum.VisitorStatus.Rejected;
                Console.WriteLine($"Visitor {visitor.Id} has been rejected");
            }

            Visitors.TryAdd(visitor.Id, visitor);
        }
    }
}
