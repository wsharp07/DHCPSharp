using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DHCPSharp.DAL.Repository;

namespace DHCPSharp
{
    public class LeaseCleanupThread
    {
        private readonly ManualResetEvent _reset;
        private bool _isRunning;
        private readonly Thread _thread;
        private readonly int _intervalMilliseconds;
        private readonly ILeaseManager _manager;

        public LeaseCleanupThread(ILeaseManager manager, int intervalMilliseconds = 60000)
        {
            _manager = manager;
            _intervalMilliseconds = intervalMilliseconds;
            _reset = new ManualResetEvent(false);
            _isRunning = false;
            _thread = new Thread(Work) {Name = "LeaseCleanupThread"};
        }

        public void Start()
        {
            _thread.Start();
            _isRunning = true;
        }

        public void Stop()
        {
            _isRunning = false;
            _reset.Set();
            _thread.Join(1000);
        }

        private async void Work()
        {
            while (_isRunning)
            {
                await _manager.CleanExpiredLeases().ConfigureAwait(false);
                _reset.WaitOne(_intervalMilliseconds);
            }
        }
    }
}
