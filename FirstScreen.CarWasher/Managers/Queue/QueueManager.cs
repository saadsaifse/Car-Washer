using FirstScreen.CarWasher.Exceptions;
using FirstScreen.CarWasher.Interfaces;
using FirstScreen.CarWasher.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static FirstScreen.CarWasher.Enums.Enum;

namespace FirstScreen.CarWasher.Managers.Queue
{
    public class QueueManager : IQueueManager
    {
        readonly ConcurrentDictionary<string, CarQueue> CarQueues = new ConcurrentDictionary<string, CarQueue>();

        public event EventHandler<QueueEventArgs> QueueNotification = delegate { };

        public QueueManager()
        {
        }

        public ICarQueue CreateQueue(int size, QueueType type)
        {
            var carQueue = new CarQueue(size, type);
            if (!CarQueues.TryAdd(carQueue.Id, carQueue))
                Console.WriteLine($"Queue {carQueue.Id} could not be added to local dictionary");
            return carQueue;
        }

        public bool TryEnqueue(string queueId, Visitor visitor)
        {
            try
            {
                if (!CarQueues.TryGetValue(queueId, out CarQueue queue))
                    throw new QueueException();
                queue.Enqueue(visitor);
                FireQueueNotification(QueueEventArgs.Create(queueId, visitor, queue.Type, QueueOperation.Enqueue));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public ICarQueue GetCarQueue(string queueId)
        {
            if (CarQueues.TryGetValue(queueId, out CarQueue queue))
                return queue;
            return null;
        }

        public bool TryOptimalEnqueue(Enums.Enum.QueueType type, Visitor visitor)
        {
            try
            {
                var optimalQueue = CarQueues.Where(queue => queue.Value.Type == type && queue.Value.Size > queue.Value.Count)
                                               .Min(queue => queue.Value);
                if (optimalQueue == null)
                    throw new QueueException("Optimal queue could not be found");

                if (!TryEnqueue(optimalQueue.Id, visitor))
                    throw new QueueException("Could not be enqueued");

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool TryDequeue(string queueId, out Visitor visitor)
        {
            if (CarQueues.TryGetValue(queueId, out CarQueue queue))
            {
                if (queue.TryDequeue(out visitor))
                {
                    FireQueueNotification(QueueEventArgs.Create(queueId, visitor, queue.Type, QueueOperation.Dequeue));
                    return true;
                }
            }
            visitor = null;
            return false;
        }

        public IEnumerable<ICarQueue> GetCarQueues(QueueType type)
        {
           return CarQueues.Values.Where(q=> q.Type == type ).ToList();
        }

        private void FireQueueNotification(QueueEventArgs args)
        {
            if (this.QueueNotification!=null)
                QueueNotification(this, args);
        }
    }
}
