using System;

namespace HintMachine.Models
{
    public class ArchipelagoConnectionException : Exception
    {
        public ArchipelagoConnectionException(string message) : base(message) 
        {}
    }

    public class ProcessRamWatcherException : Exception
    {
        public ProcessRamWatcherException(string message) : base(message)
        {}
    }
}
