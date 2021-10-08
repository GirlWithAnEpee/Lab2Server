using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SimpleServer
{
    public class Job
    {
        public IPEndPoint EndPoint { get; protected set; }
        public string Message { get; protected set; }

        public Job(string message, IPEndPoint endPoint)
        {
            EndPoint = endPoint;
            Message = message;
        }
    }
}
