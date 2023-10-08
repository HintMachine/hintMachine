using System;
using System.Security;
using System.Threading;
using System.Windows.Documents;

namespace HintMachine
{
    public class GameWatcherThread
    {
        private Thread _pollTickThread = null;

        public delegate void ThreadFunction();
        private ThreadFunction _function = null;

        public bool IsRunning { get; set; } = true;

        public GameWatcherThread(ThreadFunction function)
        {
            _function = function;
            
            _pollTickThread = new Thread(ThreadLoop);
            _pollTickThread.IsBackground = true;
            _pollTickThread.Start();
        }
        
        private void ThreadLoop()
        {
            while (IsRunning)
            {
                _function?.Invoke();
                Thread.Sleep(Globals.TickInterval);
            }
        }
    }
}
