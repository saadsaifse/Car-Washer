using FirstScreen.CarWasher.Models;
using System;
using static FirstScreen.CarWasher.Enums.Enum;

namespace FirstScreen.CarWasher.Managers.Queue
{
    public class QueueEventArgs:EventArgs
    {
        public string Id { get; set; }
        public QueueType Type { get; set; }
        public Visitor Visitor { get; set; }
        public QueueOperation Operation { get; set; }

        public static QueueEventArgs Create(string id, Visitor visitor, QueueType type, QueueOperation operation)
        {
            return new QueueEventArgs
            {
                Id = id,
                Operation = operation,
                Type = type,
                Visitor = visitor
            };
        }
    }
}
