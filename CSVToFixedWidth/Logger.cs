using System;
namespace STRSOhioAnnualReporting
{
    public class Logger
    {
        private static Logger globalLogger;

        public static Logger Instance
        {
            get
            {
                if (globalLogger == null)
                {
                    globalLogger = new Logger();
                }

                return globalLogger;
            }
        }

        public Action<string> OnWarning { get; set; }

        public Action<string> OnError { get; set; }

        private Logger()
        {
            OnWarning = (message) => Console.WriteLine($"Warning: { message }");
            OnError = (message) => Console.WriteLine($"Error: { message }");
        }

        public string Warn(string warning)
        {
            OnWarning(warning);

            return warning;
        }

        public string Error(string error)
        {
            OnError(error);

            return error;
        }
    }
}
