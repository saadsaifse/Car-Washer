namespace FirstScreen.CarWasher.Enums
{
    public class Enum
    {
        public enum QueueType
        {
            Washing = 0,
            Drying = 1
        }

        public enum BayType
        {
            Washing = 0,
            Drying = 1
        }

        public enum VisitorStatus
        {
            Rejected,
            Underprocessing,
            Processed
        }

        public enum QueueOperation
        {
            Enqueue,
            Dequeue
        }
    }
}
