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
        private const int listenPort = 8001;
        private const int sendPort = 8008;
        static IPEndPoint endPoint;
        static UdpClient listener;
        static Thread thread;
        static byte[] data;
        static List<string> logins = new List<string>();
        private static Scheduler _scheduler;
        private static bool isStopped = false;


        static void Main(string[] args)
        {
            listener = new UdpClient(listenPort);
            endPoint = new IPEndPoint(IPAddress.Any, listenPort);

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
            listener.Close();
            _scheduler.Stop();
        }

        // private static void Calculate()
        // {
        //     // получаем сообщение
        //     StringBuilder builder = new StringBuilder();
        //     builder.Append(Encoding.Unicode.GetString(data, 0, data.Length));
        //
        //     string[] parts = builder.ToString().Split(';');
        //     string login = parts[0];
        //     string message = "";
        //     if (logins.Contains(login))
        //     {
        //         string operation = parts[1];
        //         int ch1 = 0;
        //         int ch2 = 0;
        //         object result; //иначе компилятор ругается
        //         if (operation == "stop")
        //         {
        //             //остановить сервер
        //             result = null;
        //             message = "Сервер был остановлен.";
        //         }
        //         else if (int.TryParse(parts[2], out ch1))
        //         {
        //             if (parts.Length > 3 && int.TryParse(parts[3], out ch2))
        //             {
        //                 switch (operation)
        //                 {
        //                     case "+":
        //                         result = ch1 + ch2;
        //                         break;
        //                     case "-":
        //                         result = ch1 - ch2;
        //                         break;
        //                     case "*":
        //                         result = ch1 * ch2;
        //                         break;
        //                     default:
        //                         result = null;
        //                         message = "Ошибка!";
        //                         break;
        //                 }
        //             }
        //             else
        //             {
        //                 try
        //                 {
        //                     result = Factorial(ch1);
        //                     //message = result.ToString();
        //                 }
        //                 catch (Exception ex)
        //                 {
        //                     result = null;
        //                     message = ex.Message;
        //                 }
        //             }
        //         }
        //         else
        //         {
        //             result = null;
        //             message = "Некорректные данные";
        //         }
        //
        //         if (result != null)
        //             message = result.ToString();
        //     }
        //     else
        //     {
        //         message = "Ошибка авторизации";
        //     }
        //
        //     // отправляем ответ
        //     Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //     try
        //     {
        //         byte[] send = Encoding.Unicode.GetBytes(message);
        //         IPEndPoint ep = new IPEndPoint(endPoint.Address, sendPort);
        //         socket.SendTo(send, ep);
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine(ex.Message);
        //     }
        //     finally
        //     {
        //         socket.Close();
        //     }
        // }

        // static int Factorial(int x)
        // {
        //     if (x == 0)
        //     {
        //         return 1;
        //     }
        //     else
        //     {
        //         return x * Factorial(x - 1);
        //     }
        // }
    }
}