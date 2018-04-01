using System;
using System.Collections.Generic;
using System.Text;

namespace FirstScreen.CarWasher.Interfaces
{
    public interface IProcessingNotification
    {
        event EventHandler<Tuple<string, TimeSpan>> ProcessTime;
    }
}
