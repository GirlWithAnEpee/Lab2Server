using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleServer
{
    public class Scheduler
    {
        public event Action<string> MessageResolved;

        private WorkerFactory _factory;
        private Queue<string> Messages;
        private Encoding _encoding;
        private bool _acceptingMessages;

        public Scheduler(Encoding encoding, int maxThreads, LoginService loginService)
        {
            _encoding = encoding;
            _factory = new WorkerFactory(maxThreads, loginService, this);
        }

        public void Start()
        {
            _acceptingMessages = true;
            while (_acceptingMessages)
            {
                if (Messages.Count > 0 && _factory.Available > 0)
                {
                    string message = Messages.Dequeue();
                    _factory.StartJob(message);
                }
            }
        }

        public void Enqueue(byte[] data)
        {
            if (!_acceptingMessages)
                return;


            string message = _encoding.GetString(data);
            Messages.Enqueue(message);
        }

        public void Stop()
        {
            _acceptingMessages = false;
        }
    }
}