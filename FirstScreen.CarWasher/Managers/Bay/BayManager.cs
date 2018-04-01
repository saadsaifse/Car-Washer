using FirstScreen.CarWasher.Interfaces;
using FirstScreen.CarWasher.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FirstScreen.CarWasher.Managers.Bay
{
    public class BayManager: IBayManager
    {
        public event EventHandler<string> Processed = delegate { };
        public event EventHandler<Tuple<string, TimeSpan>> ProcessTime = delegate { };

        readonly ConcurrentQueue<Tuple<Visitor, AutoResetEvent>> DryingWaitingQueue = new ConcurrentQueue<Tuple<Visitor, AutoResetEvent>>();
        readonly ConcurrentDictionary<string, CarBay> Bays = new ConcurrentDictionary<string, CarBay>();

        readonly IQueueManager queueManager;
        public BayManager(IQueueManager queueManager)
        {
            this.queueManager = queueManager;
            queueManager.QueueNotification += QueueManager_QueueNotification;
        }

        public void CreateBay(ICarQueue queue, Enums.Enum.BayType type, int processingTime)
        {
            var bay = new CarBay
            {
                Id = Guid.NewGuid().ToString().Substring(0, 3).ToUpper(),
                QueueId = queue.Id,
                Type = type,
                ProcessingSeconds = processingTime,
            };
            Bays.TryAdd(bay.Id, bay);
        }

        void StartWashing(CarBay bay)
        {
            if (Bays.TryGetValue(bay.Id, out CarBay cb))
            {
                if (cb.IsBusy) return;
                else cb.IsBusy = true;
            }
            else return;

            Console.WriteLine($"Washing bay {bay.Id} has started washing cars from queue {bay.QueueId} at {DateTimeOffset.Now.TimeOfDay}");
            while (queueManager.TryDequeue(bay.QueueId, out Visitor visitor))
            { 
                Console.WriteLine($"Visitor {visitor.Id} dequeued from washing queue {bay.QueueId} at {DateTimeOffset.Now.TimeOfDay}");
                Console.WriteLine($"Washing started for visitor {visitor.Id} at {DateTimeOffset.Now.TimeOfDay}");

                Thread.Sleep(new TimeSpan(0,0, bay.ProcessingSeconds));
                NotifyAddProcessingTime(visitor.Id, TimeSpan.FromSeconds(bay.ProcessingSeconds));
                Console.WriteLine($"Washing finished for visitor {visitor.Id} at {DateTimeOffset.Now.TimeOfDay}");

                EnqueueForDrying(visitor);
            }
            Bays.Values.SingleOrDefault(b => b.Id == bay.Id).IsBusy = false;
            Console.WriteLine($"Washing bay {bay.Id} has finished washing cars from queue {bay.QueueId} at {DateTimeOffset.Now.TimeOfDay}");
        }

        void NotifyAddProcessingTime(string visitorId, TimeSpan duration)
        {
            this.ProcessTime(this, new Tuple<string, TimeSpan>(visitorId, duration));
        }

        void StartDrying(CarBay bay)
        {
            if (Bays.TryGetValue(bay.Id, out CarBay cb))
            {
                if (cb.IsBusy) return;
                else cb.IsBusy = true;
            }
            else return;


            Console.WriteLine($"Drying bay {bay.Id} has started drying cars from queue {bay.QueueId} at {DateTimeOffset.Now.TimeOfDay}");
            var dryingQueue = queueManager.GetCarQueues(Enums.Enum.QueueType.Drying).FirstOrDefault();
            while (queueManager.TryDequeue(dryingQueue.Id, out Visitor visitor))
            {
                Console.WriteLine($"Visitor {visitor.Id} dequeued from drying queue {bay.QueueId}");
                Console.WriteLine($"Drying started for visitor {visitor.Id}");

                Thread.Sleep(new TimeSpan(0, 0, bay.ProcessingSeconds));
                NotifyAddProcessingTime(visitor.Id, TimeSpan.FromSeconds(bay.ProcessingSeconds));

                Console.WriteLine($"Drying finished for visitor {visitor.Id} at {DateTimeOffset.Now.TimeOfDay}");
                Console.WriteLine($"Visitor {visitor.Id} has been processed successfuly at {DateTimeOffset.Now.TimeOfDay}");

                Processed(this, visitor.Id);
            }

            Bays.Values.SingleOrDefault(b => b.Id == bay.Id).IsBusy = false;

            Console.WriteLine($"Drying bay {bay.Id} has finished drying cars from queue {bay.QueueId} at {DateTimeOffset.Now.TimeOfDay}");
        }

        private void QueueManager_QueueNotification(object sender, Queue.QueueEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                if (e.Operation == Enums.Enum.QueueOperation.Enqueue)
                {
                    Console.WriteLine($"Visitor {e.Visitor.Id} enqueued in {e.Type.ToString()} queue {e.Id} at {DateTimeOffset.Now.TimeOfDay}");
                    if (e.Type == Enums.Enum.QueueType.Washing)
                    {
                        var bay = Bays.Values.SingleOrDefault(b => b.QueueId == e.Id);
                        StartWashing(bay);
                    }
                    else
                    {
                        var dryingBays = Bays.Values.Where(b => b.Type == Enums.Enum.BayType.Drying).ToList();
                        dryingBays.ForEach(bay => StartDrying(bay));
                    }
                }
                else if (e.Operation == Enums.Enum.QueueOperation.Dequeue && e.Type == Enums.Enum.QueueType.Drying)
                {
                    if (DryingWaitingQueue.TryPeek(out Tuple<Visitor, AutoResetEvent> queueItem))
                    {
                        if (queueManager.TryEnqueue(e.Id, queueItem.Item1))
                        {
                            if (DryingWaitingQueue.TryDequeue(out Tuple<Visitor, AutoResetEvent> q))
                                q.Item2.Set();
                        }
                    }
                }
            });
        }

        void EnqueueForDrying(Visitor visitor)
        {
            AutoResetEvent are = new AutoResetEvent(false);
            var dryingQueue = queueManager.GetCarQueues(Enums.Enum.QueueType.Drying).FirstOrDefault();

            //If local drying queue has cars then add to local drying queue else try to enque in main drying queue first
            if (DryingWaitingQueue.Count > 0 || !queueManager.TryEnqueue(dryingQueue.Id, visitor))
            {
                DryingWaitingQueue.Enqueue(new Tuple<Visitor, AutoResetEvent>(visitor, are));
                Console.WriteLine($"Visitor {visitor.Id} is waiting for drying queue {dryingQueue.Id} to accept next visitor");
                are.WaitOne();
            }
        }

    }
}
