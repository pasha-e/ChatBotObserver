using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MessengerBotObserver.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;

namespace MessengerBotObserver.Telegram
{
    public class TelegramMessageController: ILogging, IDisposable
    {

        private static string token { get; set; } = "";
        private static TelegramBotClient _bot;

        private CancellationTokenSource _cancellationToken;

        TelegramHandlers _telegramHandlers;

        public TelegramMessageController(FileLogger fileLogger)
        {
            _bot = new TelegramBotClient(token);

            _telegramHandlers = new TelegramHandlers();

            fileLogger.AddLoggingModule(_telegramHandlers);
        }

        public void Start()
        {
            //using var cts = new CancellationTokenSource();

            _cancellationToken = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            
            _bot.StartReceiving(_telegramHandlers.HandleUpdateAsync,
                _telegramHandlers.HandleErrorAsync,
                receiverOptions,
                _cancellationToken.Token);

            LogMessageReceived?.Invoke(nameof(TelegramMessageController), new LogMessage(LogMessageType.Information, "Telegram bot started..."));

            // Send cancellation request to stop bot
            //cts.Cancel();
        }

        public void Stop()
        {
            //Send cancellation request to stop bot
            _cancellationToken.Cancel();
        }

        public event Action<string, LogMessage> LogMessageReceived;

        public void Dispose()
        {
            Stop();
        }
    }
}
