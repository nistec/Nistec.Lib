using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nistec.Threading
{

    public abstract class MultiWorkerEngine
    {
        private readonly List<Task> _workers = new List<Task>();
        private bool _running;

        protected virtual void OnError(string messaage)
        {

        }

        protected virtual void WorkerScheduler()
        {

        }

        protected virtual void WorkerJob(object job, int workerId)
        {

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
                    await ProcessWorkAsync(workerId, token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    OnError(ex.Message);
                }

                await Task.Delay(100, token).ConfigureAwait(false);
            }
        }

        private async Task ProcessWorkAsync(int workerId, CancellationToken token)
        {
            // Example: fetch next job from SQL or MongoDB
            var job = await GetNextJobAsync(token).ConfigureAwait(false);
            if (job == null)
            {
                await Task.Delay(500, token).ConfigureAwait(false);
                return;
            }

            // Process the job
            await ExecuteJobAsync(job, workerId, token).ConfigureAwait(false);
        }

        private Task<object> GetNextJobAsync(CancellationToken token)
        {
            WorkerScheduler();
            // Replace with your scheduler logic
            return Task.FromResult<object>(null);
        }

        private Task ExecuteJobAsync(object job, int workerId, CancellationToken token)
        {

            // Replace with your job execution logic
            return Task.CompletedTask;
        }
    }
}
