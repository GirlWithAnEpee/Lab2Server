using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleServer
{
    class Scheduler
    {
        private WorkerFactory _factory;
        private Queue<Job> Messages;
        private Encoding _encoding;
        private bool _acceptingMessages;

        public Scheduler(Encoding encoding, WorkerFactory factory)
        {
            _encoding = encoding;
            _factory = factory;
            Messages = new Queue<Job>();
        }

        /// <summary>
        /// Начинает в фоновом потоке забирать задачи из очереди и отдавать на выполнение
        /// </summary>
        public void Start()
        {
            _acceptingMessages = true;
            Task.Factory.StartNew(ResolveMessages);
        }

        /// <summary>
        /// Добавляет задачу в очередь обработки
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endPoint"></param>
        public void Enqueue(byte[] data, IPEndPoint endPoint)
        {
            if (!_acceptingMessages)
                return;

            string message = _encoding.GetString(data);
            Messages.Enqueue(new Job(message, endPoint));
        }

        /// <summary>
        /// Останавливает приём новых задач и выполняет оставшиеся
        /// </summary>
        public void Stop()
        {
            _acceptingMessages = false;
        }

        private void ResolveMessages()
        {
            while (_acceptingMessages || Messages.Count > 0)
            {
                if (Messages.Count > 0 && _factory.Available > 0)
                {
                    Job message = Messages.Dequeue();
                    _factory.StartJob(message);
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }
    }
}
