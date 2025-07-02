using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Quickstarts.ConsoleReferenceClient
{
    public class OpcLogger : ILogger
    {
        private readonly string _logFilePath;
        private static object _lock = new object();

        public readonly LogLevel Level;
        public readonly LogLevel ConsoleLevel;

        public OpcLogger() { }
        private OpcLogger(string filePath)
        {
            _logFilePath = filePath;
        }
        public OpcLogger(string filePath, LogLevel level) : this(filePath)
        {
            Level = level;
            ConsoleLevel = level;
        }

        public OpcLogger(string filePath, LogLevel level, LogLevel consoleLevel) : this(filePath, level)
        {
            ConsoleLevel = consoleLevel;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= Level;
        }

        public bool ConsoleIsEnabled(LogLevel logLevel)
        {
            return logLevel >= ConsoleLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            if (formatter != null)
            {
                lock (_lock)
                {
                    var n = Environment.NewLine;
                    //string exc = "";
                    //if (exception != null) exc = n + exception.GetType() + ": " + exception.Message + n + exception.StackTrace + n;
                    //string logText = logLevel.ToString() + ": " + DateTime.Now.ToString() + " " + formatter(state, exception) + n + exc;
                    string logText = formatter(state, exception) + n;
                    /*if (ConsoleIsEnabled(logLevel))
                    {
                        Console.Write(logText);
                    }*/
                    File.AppendAllText(_logFilePath, logText);
                }
            }
        }
    }
}
