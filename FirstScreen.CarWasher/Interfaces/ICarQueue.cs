using static FirstScreen.CarWasher.Enums.Enum;

namespace FirstScreen.CarWasher.Interfaces
{
    public interface ICarQueue
    {
        QueueType Type { get; }
        string Id { get; }
        int Size { get; }
    }
}