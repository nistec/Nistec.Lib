using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nistec.Threading
{

    public class HighWorkerEngine<T> where T:class
    {
        private readonly List<Task> _workers = new List<Task>();
        private bool _running;

        long _ActiveConnections;
        int _MaxConnection=10;
        public HighWorkerEngine(int maxConnection)
        {
            _MaxConnection = maxConnection;
        }

        protected virtual void OnError(string messaage)
        {

        }

        protected virtual Task<T> OnClaimJobAsync(CancellationToken token)
        {
            // Must be atomic: SQL or MongoDB must guarantee only one worker gets the job
            return Task.FromResult<T>(null);
        }

        protected virtual Task OnProcessJobAsync(T job, int workerId, CancellationToken token)
        {
            // Your job logic here
            return Task.CompletedTask;
        }

        public void Start(int workerCount, CancellationToken token)
        {
            _running = true;

            for (int i = 0; i < workerCount; i++)
            {
                int workerId = i;
                _workers.Add(Task.Run(() => WorkerLoop(workerId, token), token));
            }
        }

        public void Stop()
        {
            _running = false;

            try
            {
                Task.WaitAll(_workers.ToArray(), TimeSpan.FromSeconds(10));
            }
            catch { }
        }

        private async Task WorkerLoop(int workerId, CancellationToken token)
        {
            while (_running && !token.IsCancellationRequested)
            {
                try
                {

                    if (Interlocked.Read(ref _ActiveConnections) > _MaxConnection)
                    {
                        await Task.Delay(100).ConfigureAwait(false);
                        continue;
                    }
                    Interlocked.Increment(ref _ActiveConnections);

                    var job = await ClaimJobAsync(token).ConfigureAwait(false);
                    if (job == null)
                    {
                        await Task.Delay(5, token).ConfigureAwait(false);
                        continue;
                    }

                    await ProcessJobAsync(job, workerId, token).ConfigureAwait(false);
                    await Task.Delay(100).ConfigureAwait(false);

                }
                catch (Exception ex)
                {
                    OnError(ex.Message);
                }
                finally
                {
                    Interlocked.Decrement(ref _ActiveConnections);
                }
            }
        }

        private Task<T> ClaimJobAsync(CancellationToken token)
        {
            return OnClaimJobAsync(token);
            // Must be atomic: SQL or MongoDB must guarantee only one worker gets the job
            //return Task.FromResult<T>(null);
        }

        private Task ProcessJobAsync(T job, int workerId, CancellationToken token)
        {
            OnProcessJobAsync(job, workerId, token);
            // Your job logic here
            return Task.CompletedTask;
        }
    }

    //public class JobItem
    //{
    //    public int Id { get; set; }
    //}
}
