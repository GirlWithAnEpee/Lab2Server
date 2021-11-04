using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleServer
{
    public class WorkerFactory
    {
        public event Action StopSignal;
        public event Action<Job> MessageResolved;
        public int Available { get; protected set; }
        public int MaxThreads { get; }

        private readonly TaskFactory _taskFactory;
        private readonly List<Worker> Workers;
        private readonly LoginService _loginService;
        private readonly SynchronizationContext _ctx;

        public WorkerFactory(int maxThreads, LoginService loginService)
        {
            _taskFactory = new TaskFactory();
            _loginService = loginService;
            MaxThreads = maxThreads;
            Workers = new List<Worker>(maxThreads);
            Available = MaxThreads;
            _ctx = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        /// <summary>
        /// Пытается отдать задачу на выполнению работникам
        /// </summary>
        /// <param name="job"></param>
        public void StartJob(Job job)
        {
            if (Available > 0)
            {
                Available -= 1;

                var worker = new Worker(_loginService, _taskFactory);
                Workers.Add(worker);
                worker.Finished += WorkerFinished;
                worker.Start(job);
            }
        }

        private void WorkerFinished(Worker worker, Job result)
        {
            Workers.Remove(worker);
            Available += 1;

            if (result.Message == "stop")
                _ctx.Post((_) => StopSignal?.Invoke(), null);

            _ctx.Post((_) => MessageResolved?.Invoke(result), null);
        }
    }
}
