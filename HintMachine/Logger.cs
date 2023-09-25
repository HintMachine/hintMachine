using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace HintMachine
{
    public enum LogMessageType
    {
        RAW = 0,
        INFO = 1,
        WARNING = 2,
        ERROR = 3,
        HINT = 4,
        CHAT = 5,
        ITEM_RECEIVED = 6,
        ITEM_SENT = 7,
        GOAL = 8,
        SERVER_RESPONSE = 9,
        JOIN_LEAVE = 10,
    }

    public abstract class Logger
    {
        public delegate void OnMessageLoggedCallback(string message, LogMessageType type);

        public static OnMessageLoggedCallback OnMessageLogged = null;

        public static void Log(string message, LogMessageType logMessageType = LogMessageType.RAW)
        {
            Console.WriteLine(message);

            if(OnMessageLogged != null)
                OnMessageLogged(message, logMessageType);
        }

        public static void Info(string message)
        {
            Log(message, LogMessageType.INFO);
        }

        public static void Warn(string message)
        {
            Log(message, LogMessageType.WARNING);
        }

        public static void Error(string message)
        {
            Log(message, LogMessageType.ERROR);
        }

        public static void Hint(string message)
        {
            Log(message, LogMessageType.HINT);
        }
    }


}
