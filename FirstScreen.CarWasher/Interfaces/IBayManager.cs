using System;
using System.Collections.Generic;
using System.Text;

namespace FirstScreen.CarWasher.Interfaces
{
    public interface IBayManager: IProcessingNotification
    {
        void CreateBay(ICarQueue queue, Enums.Enum.BayType type, int processingTime);
        event EventHandler<string> Processed;
    }
}
