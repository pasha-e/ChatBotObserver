using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessengerBotObserver.Properties;
using Serilog;

namespace MessengerBotObserver.Diagnostics
{
    public class FileLogger : IDisposable
    {
        private bool _initialized;

        private bool _disposed;

        public FileLogger()
        {
            Initialize();
        }

        private void Initialize()
        {
            _initialized = false;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(GetFilePath("appLog-.txt"), rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            _initialized = true;
        }

        public void Dispose()
        {
            _disposed = true;
        }

        public void AddLoggingModule(ILogging module)
        {
            module.LogMessageReceived += AddMessage;
        }

        public void AddMessage(string module, LogMessage message)
        {
            if (_disposed)
                throw new InvalidOperationException("Object disposed");

            if (!_initialized)
            {
                Initialize();
            }

            
            switch (message.Type)
            {
                case LogMessageType.Debug:
                    Log.Logger.Debug($" {module}: {(string.IsNullOrEmpty(message.ExtendedMessage) ? message.Message : message.ExtendedMessage)}");
                    break;
                case LogMessageType.Information:
                    Log.Logger.Information($" {module}: {(string.IsNullOrEmpty(message.ExtendedMessage) ? message.Message : message.ExtendedMessage)}");
                    break;
                case LogMessageType.Warning:
                    Log.Logger.Warning($" {module}: {(string.IsNullOrEmpty(message.ExtendedMessage) ? message.Message : message.ExtendedMessage)}");
                    break;
                case LogMessageType.Error:
                    Log.Logger.Error($" {module}: {(string.IsNullOrEmpty(message.ExtendedMessage) ? message.Message : message.ExtendedMessage)}");
                    break;
            }
        }

        private string GetFilePath(string fileName)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", fileName);

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            return filePath;
        }
    }
}
