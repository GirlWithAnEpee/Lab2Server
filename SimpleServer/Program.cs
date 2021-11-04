using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;

namespace SimpleServer
{
    class Program
    {
        private const int listenPort = 8080;
        private const int sendPort = 8888;
        private const int listenPortBC = 1010;
        private const int sendPortBC = 1111;
        static DiscoveryClient client;
        static IPEndPoint endPoint;
        static UdpClient listener;
        static byte[] data;
        static List<string> logins = new List<string>();
        private static Scheduler _scheduler;
        private static bool isStopped = false;


        static void Main(string[] args)
        {
            listener = new UdpClient(listenPort);
            endPoint = new IPEndPoint(IPAddress.Any, sendPort);
            client = new DiscoveryClient(Guid.NewGuid().ToString(), listenPortBC, sendPortBC);//1010 и 1111
            client.StartDiscovery(revealSelf: true, discover: false);

            ReadLogins("logins.txt");
            LoginService loginService = new LoginService(logins);
            WorkerFactory factory = new WorkerFactory(4, loginService);
            _scheduler = new Scheduler(Encoding.Unicode, factory);

            factory.StopSignal += Stop;
            factory.MessageResolved += SendResponse;
            _scheduler.Start();

            Console.WriteLine("Сервер запущен. Ожидание подключений...");
            try
            {
                while (!isStopped)
                {
                    data = listener.Receive(ref endPoint);
                    _scheduler.Enqueue(data, endPoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                listener.Close();
            }
        }

        private static void SendResponse(Job response)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(response.Message);
            listener.Send(bytes, bytes.Length, response.EndPoint);
        }

        private static void ReadLogins(string loginsFile)
        {
            using (var sr = new StreamReader(loginsFile))
            {
                while (!sr.EndOfStream)
                    logins.Add(sr.ReadLine());
            }
        }

        private static void Stop()
        {
            isStopped = true;
            Console.WriteLine("Сервер завершает работу...");
            listener.Close();
            _scheduler.Stop();
        }
    }
}
