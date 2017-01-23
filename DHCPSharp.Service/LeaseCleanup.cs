using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DHCPSharp.DAL.Repository;

namespace DHCPSharp
{
    public class LeaseCleanup : ILeaseCleanup
    {
        private readonly ILeaseManager _manager;
        readonly CancellationTokenSource _cancellation;

        public LeaseCleanup(ILeaseManager manager)
        {
            _manager = manager;
            _cancellation = new CancellationTokenSource();

            Task.Run(() => Work(_cancellation.Token));
        }

        private async Task Work(CancellationToken token)
        {
            try
            {
                while (!_cancellation.IsCancellationRequested)
                {
                    try
                    {
                        await _manager.CleanExpiredLeases()
                            .ConfigureAwait(false);
                    }
                    catch { }

                    await Task.Delay(10000, token)
                        .ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException) { }
        }
    }
}
