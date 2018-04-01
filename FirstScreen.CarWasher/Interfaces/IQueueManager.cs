using FirstScreen.CarWasher.Managers.Queue;
using FirstScreen.CarWasher.Models;
using System;
using System.Collections.Generic;
using static FirstScreen.CarWasher.Enums.Enum;

namespace FirstScreen.CarWasher.Interfaces
{
    public interface IQueueManager
    {
        event EventHandler<QueueEventArgs> QueueNotification;

        ICarQueue CreateQueue(int size, QueueType type);
        ICarQueue GetCarQueue(string queueId);
        IEnumerable<ICarQueue> GetCarQueues(QueueType type);

        bool TryEnqueue(string queueId, Visitor visitor);
        bool TryOptimalEnqueue(QueueType type, Visitor visitor);
        bool TryDequeue(string queueId, out Visitor visitor);
    }
}
