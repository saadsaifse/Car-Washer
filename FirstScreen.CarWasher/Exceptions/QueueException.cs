using System;
using System.Collections.Generic;
using System.Text;

namespace FirstScreen.CarWasher.Exceptions
{
    public class QueueException:Exception
    {
        public QueueException() : base()
        {

        }

        public QueueException(string message):base(message)
        {

        }

    }
}
