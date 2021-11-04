using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleServer
{
    public class Worker
    {
        public Action<Worker, Job> Finished;

        public Task<string> Task { get; protected set; }
        public bool IsFinished { get; protected set; } = false;

        private LoginService _loginService;
        private TaskFactory _taskFactory;
        private IPEndPoint _endPoint;

        public Worker(LoginService loginService, TaskFactory factory)
        {
            _taskFactory = factory;
            _loginService = loginService;
        }

        public void Start(Job job)
        {
            _endPoint = job.EndPoint;
            Task = _taskFactory.StartNew(() => Calculate(job.Message));
            Task.ContinueWith(SetFinished);
        }

        private async Task SetFinished(Task<string> task)
        {
            IsFinished = true;
            string result = await task;
            Finished?.Invoke(this, new Job(result, _endPoint));
        }

        private string Calculate(string data)
        {
            string[] parts = data.Split(';');
            int ch1 = 0;
            int ch2 = 0;

            string login = parts[0];
            if (!_loginService.IsLogged(login))
                return "Ошибка авторизации";

            string operation = parts[1];

            //остановить сервер
            if (operation == "stop")
                return "stop";

            if (int.TryParse(parts[2], out ch1))
            {
                if (parts[3]=="")
                    return CalculateFactorial(ch1);

                if (parts.Length > 3 && int.TryParse(parts[3], out ch2))
                    return CalculateNumbers(operation, ch1, ch2);
            }

            return "Некорректные данные";
        }

        private static string CalculateNumbers(string operation, int ch1, int ch2)
        {
            int? result;
            switch (operation)
            {
                case "+":
                    result = ch1 + ch2;
                    break;
                case "-":
                    result = ch1 - ch2;
                    break;
                case "*":
                    result = ch1 * ch2;
                    break;
                default:
                    result = null;
                    break;
            }

            if (result != null)
                return result.ToString();

            return "Ошибка";
        }

        private static string CalculateFactorial(int ch1)
        {
            try
            {
                return Factorial(ch1).ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static int Factorial(int x)
        {
            if (x == 0)
                return 1;
            else
                return x * Factorial(x - 1);
        }
    }
}
