using FirstScreen.CarWasher.Exceptions;
using FirstScreen.CarWasher.Interfaces;
using FirstScreen.CarWasher.Models;
using System;
using System.Collections.Concurrent;
using static FirstScreen.CarWasher.Enums.Enum;

namespace FirstScreen.CarWasher.Managers.Queue
{
    public class CarQueue : ConcurrentQueue<Visitor>, ICarQueue, IComparable<CarQueue>
    {
        public string Id { get; private set; }
        public QueueType Type { get; private set; }
        private readonly object syncObject = new object();

        public int Size { get; private set; }

        public CarQueue(int size, QueueType type)
        {
            Size = size;
            Type = type;
            Id = Guid.NewGuid().ToString().Substring(0, 3).ToUpper();
        }

        public new void Enqueue(Visitor visitor)
        {
            lock (syncObject)
            {
                if (Count >= Size)
                    throw new QueueException($"Queue is full having {Count} visitors");
            }

            base.Enqueue(visitor);
        }

        /// <summary>
        /// Compares type and count only as we are not concerened whether the capacity is more or less. 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(CarQueue other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            if (Type == other.Type)
            {
                return Count.CompareTo(other.Count);
            }
            return 1;
        }
    }
}
