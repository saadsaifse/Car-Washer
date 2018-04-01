using static FirstScreen.CarWasher.Enums.Enum;

namespace FirstScreen.CarWasher.Models
{
    public class CarBay
    {
        public string Id { get; set; }
        public BayType Type { get; set; }
        public int ProcessingSeconds { get; set; }
        public bool IsBusy { get; set; }
        public string QueueId { get; set; }
    }
}
