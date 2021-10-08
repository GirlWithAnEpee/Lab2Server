using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleServer
{
    public class WorkerFactory
    {
        public event Action StopSignal;
        public int Available { get; }
        public int MaxThreads { get; }

        private TaskFactory _taskFactory;

        private List<Worker> Workers;
        private LoginService _loginService;

        public WorkerFactory(int maxThreads, LoginService loginService)
        {
            _taskFactory = new TaskFactory();
            _loginService = loginService;
            MaxThreads = maxThreads;
            Workers = new List<Worker>(maxThreads);
        }

        public void StartJob(string message)
        {
            if (Available > 0)
            {
                var worker = new Worker(_loginService, _taskFactory);
                Workers.Add(worker);
                worker.Finished += WorkerFinished;
                worker.Start(message);
            }
        }

        private void WorkerFinished(Worker worker, string result)
        {
            Workers.Remove(worker);

            if (result == "stop")
                StopSignal?.Invoke();
        }
    }
}