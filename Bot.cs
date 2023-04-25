using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using TelegramBot_HoursStore.Commands;

namespace TelegramBot_HoursStore.Services
{
    class Bot
    {
        private static TelegramBotClient client;

        private static List<Command> commandsList;
        public static IReadOnlyList<Command> Commands { get => commandsList.AsReadOnly(); }

        private static DBService _db;

        ~Bot()
        {
            _db.CloseConnection();
        }
        public static async Task<TelegramBotClient> Get(DBService db)
        {
            _db = db;

            if (client != null)
            {
                return client;
            }

            commandsList = new List<Command>
            {
                    new Start(_db)
            };


            client = new TelegramBotClient(BotSettings.Key);
            var cts = new CancellationTokenSource();
            await client.ReceiveAsync(new DefaultUpdateHandler(updateHandler: HandleUpdateAsync, errorHandler: UpdateManager.HandleErrorAsync), cancellationToken: cts.Token);

            return client;
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient arg1, Update update, CancellationToken cancellationToken)
        {
            await UpdateManager.Handle(arg1, update, cancellationToken, Commands, client, _db);
        }
    }
}
